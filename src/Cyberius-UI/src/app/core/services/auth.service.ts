import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, shareReplay, switchMap } from 'rxjs';
import {
  LoginRequest,
  RegisterRequest,
  RefreshTokenRequest,
  AuthResponse,
  User,
  AuthState,
} from '../models/auth.model';

const ACCESS_TOKEN_KEY = 'blog_access_token';
const REFRESH_TOKEN_KEY = 'blog_refresh_token';
const USER_KEY = 'blog_user';

// Буфер до истечения токена — обновляем за 30 секунд до конца
const EXPIRY_BUFFER_MS = 30_000;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  // API URL — замените на свой
  readonly API = 'http://localhost:5273/api';

  // ── State ──────────────────────────────────────────────────────
  private state = signal<AuthState>(this.loadFromStorage());

  // Идёт ли сейчас refresh (чтобы не дублировать запросы)
  private refreshInProgress$: Observable<AuthResponse> | null = null;

  // Публичные computed сигналы
  readonly user = computed(() => this.state().user);
  readonly isAuthenticated = computed(() => this.state().isAuthenticated);
  readonly accessToken = computed(() => this.state().accessToken);
  readonly refreshToken = computed(() => this.state().refreshToken);

  readonly initials = computed(() => {
    const u = this.state().user;
    if (!u) return '';
    if (u.userName) {
      return u.userName
        .split(' ')
        .map((w) => w[0])
        .slice(0, 2)
        .join('')
        .toUpperCase();
    }
    return u.email[0].toUpperCase();
  });

  // ── Проверка истечения ─────────────────────────────────────────

  /** true — токен уже истёк или истечёт в течение EXPIRY_BUFFER_MS */
  isAccessTokenExpiredOrExpiring(): boolean {
    const token = this.state().accessToken;
    if (!token) return true;
    try {
      const payload = this.decodePayload(token);
      return Number(payload['exp']) * 1000 < Date.now() + EXPIRY_BUFFER_MS;
    } catch {
      return true;
    }
  }

  isRefreshTokenExpired(): boolean {
    // Refresh token — это непрозрачная строка, не JWT.
    // Мы не можем декодировать его — просто проверяем наличие.
    return !this.state().refreshToken;
  }

  // ── Login ──────────────────────────────────────────────────────
  login(body: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API}/auth/login`, body).pipe(
      tap((res) => this.setState(res.accessToken, res.refreshToken)),
      catchError((err) => throwError(() => err)),
    );
  }

  // ── Register ───────────────────────────────────────────────────
  register(body: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API}/auth/register`, body).pipe(
      tap((res) => this.setState(res.accessToken, res.refreshToken)),
      catchError((err) => throwError(() => err)),
    );
  }

  // ── Refresh ────────────────────────────────────────────────────
  // shareReplay(1) — если несколько запросов попали на 401 одновременно,
  // все ждут одного refresh, а не посылают N параллельных запросов
  refresh(): Observable<AuthResponse> {
    if (this.refreshInProgress$) return this.refreshInProgress$;

    const { accessToken, refreshToken } = this.state();

    if (!accessToken || !refreshToken) {
      return throwError(() => new Error('No tokens'));
    }

    const body: RefreshTokenRequest = { accessToken, refreshToken };

    this.refreshInProgress$ = this.http
      .post<AuthResponse>(`${this.API}/auth/refresh-token`, body)
      .pipe(
        tap((res) => {
          this.setState(res.accessToken, res.refreshToken);
          this.refreshInProgress$ = null;
        }),
        catchError((err) => {
          this.refreshInProgress$ = null;
          this.clearState();
          return throwError(() => err);
        }),
        shareReplay(1),
      );

    return this.refreshInProgress$;
  }

  // ── Logout ─────────────────────────────────────────────────────
  // Вызывает API endpoint с userId, потом очищает локальное состояние
  logout(): void {
    const userId = this.state().user?.userId;

    if (userId && this.isAuthenticated()) {
      // fire-and-forget — даже если запрос упадёт, локально всё равно выходим
      this.http
        .post(`${this.API}/auth/logout/${userId}`, {})
        .pipe(catchError(() => throwError(() => null)))
        .subscribe({ error: () => {} });
    }

    this.clearState();
  }

  // ── Helpers ────────────────────────────────────────────────────
  private setState(accessToken: string, refreshToken: string): void {
    const user = this.decodeUser(accessToken);
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this.state.set({ user, accessToken, refreshToken, isAuthenticated: true });
  }

  private clearState(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.state.set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false });
  }

  // base64url → base64 → JSON
  // JWT использует base64url: '-' вместо '+', '_' вместо '/', без '=' padding
  // atob() понимает только стандартный base64 — нужно конвертировать
  private decodePayload(token: string): Record<string, unknown> {
    const base64url = token.split('.')[1];
    const base64 = base64url
      .replace(/-/g, '+')
      .replace(/_/g, '/')
      .padEnd(base64url.length + ((4 - (base64url.length % 4)) % 4), '=');
    return JSON.parse(atob(base64));
  }

  private loadFromStorage(): AuthState {
    try {
      const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);
      const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
      const userRaw = localStorage.getItem(USER_KEY);

      if (!accessToken || !refreshToken || !userRaw) {
        return { user: null, accessToken: null, refreshToken: null, isAuthenticated: false };
      }

      const accessExpired = this.isTokenExpired(accessToken);
      // Refresh token — опaque строка, не JWT. Истечение контролирует сервер.
      // Просто проверяем что он есть.
      const refreshExpired = !refreshToken;

      // Попытаемся декодировать для дополнительной диагностики
      try {
        const payload = this.decodePayload(accessToken);
      } catch (e) {
        console.error('[Auth] Failed to decode access token', e);
      }

      if (refreshExpired) {
        return { user: null, accessToken: null, refreshToken: null, isAuthenticated: false };
      }

      const user = JSON.parse(userRaw) as User;
      const state: AuthState = {
        user,
        accessToken: accessExpired ? null : accessToken,
        refreshToken,
        isAuthenticated: true,
      };
      return state;
    } catch (e) {
      return { user: null, accessToken: null, refreshToken: null, isAuthenticated: false };
    }
  }

  // Декодируем JWT payload из base64url — без сторонних библиотек
  private decodeUser(token: string): User {
    try {
      const payload = this.decodePayload(token);
      return {
        userId: String(
          payload['sub'] ??
            payload['nameid'] ??
            payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ??
            '',
        ),
        email: String(payload['email'] ?? ''),
        userName:
          payload['name'] != null
            ? String(payload['name'])
            : payload['given_name'] != null
              ? String(payload['given_name'])
              : undefined,
      };
    } catch {
      return { userId: '', email: '' };
    }
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = this.decodePayload(token);
      return Number(payload['exp']) * 1000 < Date.now();
    } catch {
      return true;
    }
  }
}
