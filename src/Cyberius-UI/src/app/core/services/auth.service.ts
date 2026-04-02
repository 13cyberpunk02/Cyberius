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
  UserProfile,
  AuthState,
} from '../models/auth.model';
import { ToastService } from './toast.service';

const ACCESS_TOKEN_KEY = 'blog_access_token';
const REFRESH_TOKEN_KEY = 'blog_refresh_token';
const USER_KEY = 'blog_user';
const PROFILE_KEY = 'blog_profile';

const EXPIRY_BUFFER_MS = 30_000;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private toast = inject(ToastService);

  readonly API = 'http://localhost:5273/api';
  readonly FILES_BASE = 'http://localhost:5273/api/files/';

  readonly avatarUrl = computed(() => {
    const path = this.state().profile?.avatarUrl;
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return this.FILES_BASE + path;
  });

  // ── State ──────────────────────────────────────────────────────
  private state = signal<AuthState>(this.loadFromStorage());
  refreshInProgress$: Observable<AuthResponse> | null = null;

  readonly user = computed(() => this.state().user);
  readonly profile = computed(() => this.state().profile);
  readonly isAuthenticated = computed(() => this.state().isAuthenticated);
  readonly accessToken = computed(() => this.state().accessToken);

  readonly initials = computed(() => {
    const p = this.state().profile;
    if (p) return ((p.firstName?.[0] ?? '') + (p.lastName?.[0] ?? '')).toUpperCase();
    const u = this.state().user;
    if (!u) return '?';
    return u.userName
      ? u.userName
          .split(' ')
          .map((w: string) => w[0])
          .slice(0, 2)
          .join('')
          .toUpperCase()
      : (u.email?.[0] ?? '?').toUpperCase();
  });

  readonly displayName = computed(() => {
    const p = this.state().profile;
    if (p) return `${p.firstName} ${p.lastName}`.trim();
    return this.state().user?.userName ?? this.state().user?.email ?? '';
  });

  // ── Token checks ───────────────────────────────────────────────
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
    return !this.state().refreshToken;
  }

  // ── Login ──────────────────────────────────────────────────────
  login(body: LoginRequest): Observable<UserProfile> {
    return this.http.post<AuthResponse>(`${this.API}/auth/login`, body).pipe(
      tap((res) => this.setTokens(res.accessToken, res.refreshToken)),
      switchMap(() => this.fetchMe()),
      catchError((err) => throwError(() => err)),
    );
  }

  // ── Register ───────────────────────────────────────────────────
  register(body: RegisterRequest): Observable<UserProfile> {
    return this.http.post<AuthResponse>(`${this.API}/auth/register`, body).pipe(
      tap((res) => this.setTokens(res.accessToken, res.refreshToken)),
      switchMap(() => this.fetchMe()),
      catchError((err) => throwError(() => err)),
    );
  }

  // ── Fetch /me ──────────────────────────────────────────────────
  fetchMe(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.API}/users/me`).pipe(
      tap((profile) => {
        localStorage.setItem(PROFILE_KEY, JSON.stringify(profile));
        this.state.update((s) => ({ ...s, profile }));
      }),
      catchError((err) => throwError(() => err)),
    );
  }

  // ── Refresh tokens ─────────────────────────────────────────────
  refresh(): Observable<AuthResponse> {
    if (this.refreshInProgress$) return this.refreshInProgress$;

    const refreshToken = this.state().refreshToken;
    if (!refreshToken) return throwError(() => new Error('No refresh token'));

    // accessToken в state() мог быть обнулён если истёк при загрузке страницы,
    // но в localStorage он ещё есть (истёкший) — сервер его принимает для валидации пары
    const accessToken = this.state().accessToken ?? localStorage.getItem('blog_access_token') ?? '';

    const body: RefreshTokenRequest = { accessToken, refreshToken };

    this.refreshInProgress$ = this.http
      .post<AuthResponse>(`${this.API}/auth/refresh-token`, body)
      .pipe(
        tap((res) => {
          this.setTokens(res.accessToken, res.refreshToken);
          this.refreshInProgress$ = null;
        }),
        catchError((err) => {
          this.refreshInProgress$ = null;
          // Только при явном отказе сервера (401/403) — сессия точно истекла
          // При сетевых ошибках (status 0, 503) — НЕ разлогиниваем
          if (err?.status === 401 || err?.status === 403) {
            this.clearState();
            this.toast.info('Сессия истекла — войдите снова');
          }
          return throwError(() => err);
        }),
        shareReplay(1),
      );

    return this.refreshInProgress$;
  }

  // ── Logout ─────────────────────────────────────────────────────
  logout(): void {
    const userId = this.state().user?.id;

    if (userId && this.isAuthenticated()) {
      this.http
        .post(`${this.API}/auth/logout/${userId}`, {})
        .pipe(catchError(() => throwError(() => null)))
        .subscribe({ error: () => {} });
    }

    this.clearState();
    this.router.navigate(['/']);
  }

  // ── Helpers ────────────────────────────────────────────────────
  private setTokens(accessToken: string, refreshToken: string): void {
    const user = this.decodeUser(accessToken);
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this.state.update((s) => ({ ...s, user, accessToken, refreshToken, isAuthenticated: true }));
  }

  clearState(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(PROFILE_KEY);
    this.state.set({
      user: null,
      profile: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
    });
  }

  private loadFromStorage(): AuthState {
    try {
      const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);
      const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
      const userRaw = localStorage.getItem(USER_KEY);
      const profileRaw = localStorage.getItem(PROFILE_KEY);

      // Без refresh token — точно не авторизован
      if (!refreshToken || !userRaw) return this.emptyState();

      const user = JSON.parse(userRaw) as User;
      const profile = profileRaw ? (JSON.parse(profileRaw) as UserProfile) : null;

      // Access token мог истечь — это нормально.
      // isAuthenticated остаётся true, interceptor обновит через refresh
      const accessExpired = accessToken ? this.isTokenExpired(accessToken) : true;

      return {
        user,
        profile,
        accessToken: accessExpired ? null : accessToken,
        refreshToken,
        isAuthenticated: true, // держим true, пока есть refresh token
      };
    } catch {
      return this.emptyState();
    }
  }

  private emptyState(): AuthState {
    return {
      user: null,
      profile: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
    };
  }

  private decodePayload(token: string): Record<string, unknown> {
    const base64url = token.split('.')[1];
    const base64 = base64url
      .replace(/-/g, '+')
      .replace(/_/g, '/')
      .padEnd(base64url.length + ((4 - (base64url.length % 4)) % 4), '=');
    return JSON.parse(atob(base64));
  }

  private decodeUser(token: string): User {
    try {
      const p = this.decodePayload(token);
      return {
        id: String(
          p['sub'] ??
            p['nameid'] ??
            p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ??
            '',
        ),
        email: String(p['email'] ?? ''),
        userName: p['name'] != null ? String(p['name']) : undefined,
      };
    } catch {
      return { id: '', email: '' };
    }
  }

  private isTokenExpired(token: string): boolean {
    try {
      const p = this.decodePayload(token);
      return Number(p['exp']) * 1000 < Date.now();
    } catch {
      return true;
    }
  }
}
