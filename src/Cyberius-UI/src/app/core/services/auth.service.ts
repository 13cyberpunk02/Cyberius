import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap, catchError, throwError } from 'rxjs';
import { LoginRequest, RegisterRequest, AuthResponse, User, AuthState } from '../models/auth.model';

const ACCESS_TOKEN_KEY = 'blog_access_token';
const REFRESH_TOKEN_KEY = 'blog_refresh_token';
const USER_KEY = 'blog_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  // API URL — замените на свой
  private readonly API = 'http://localhost:5273/api';

  // ── State ──────────────────────────────────────────────────────
  private state = signal<AuthState>(this.loadFromStorage());

  // Публичные readonly сигналы
  readonly user = computed(() => this.state().user);
  readonly isAuthenticated = computed(() => this.state().isAuthenticated);
  readonly accessToken = computed(() => this.state().accessToken);

  // Инициалы пользователя для аватара
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

  // ── Login ──────────────────────────────────────────────────────
  login(body: LoginRequest) {
    return this.http.post<AuthResponse>(`${this.API}/auth/login`, body).pipe(
      tap((res) => {
        const user = this.decodeUser(res.accessToken);
        this.setState(user, res.accessToken, res.refreshToken);
      }),
      catchError((err) => throwError(() => err)),
    );
  }

  // ── Register ───────────────────────────────────────────────────
  register(body: RegisterRequest) {
    return this.http.post<AuthResponse>(`${this.API}/auth/register`, body).pipe(
      tap((res) => {
        const user = this.decodeUser(res.accessToken);
        this.setState(user, res.accessToken, res.refreshToken);
      }),
      catchError((err) => throwError(() => err)),
    );
  }

  // ── Logout ─────────────────────────────────────────────────────
  logout(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.state.set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false });
  }

  // ── Refresh ────────────────────────────────────────────────────
  refresh() {
    const refreshToken = this.state().refreshToken;
    if (!refreshToken) return throwError(() => new Error('No refresh token'));

    return this.http.post<AuthResponse>(`${this.API}/auth/refresh`, { refreshToken }).pipe(
      tap((res) => {
        const user = this.decodeUser(res.accessToken);
        this.setState(user, res.accessToken, res.refreshToken);
      }),
      catchError((err) => {
        this.logout();
        return throwError(() => err);
      }),
    );
  }

  // ── Helpers ────────────────────────────────────────────────────
  private setState(user: User, accessToken: string, refreshToken: string): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this.state.set({ user, accessToken, refreshToken, isAuthenticated: true });
  }

  private loadFromStorage(): AuthState {
    try {
      const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);
      const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
      const userRaw = localStorage.getItem(USER_KEY);

      if (!accessToken || !refreshToken || !userRaw) {
        return { user: null, accessToken: null, refreshToken: null, isAuthenticated: false };
      }

      // Проверяем не истёк ли access token
      if (this.isTokenExpired(accessToken)) {
        return { user: null, accessToken: null, refreshToken, isAuthenticated: false };
      }

      const user = JSON.parse(userRaw) as User;
      return { user, accessToken, refreshToken, isAuthenticated: true };
    } catch {
      return { user: null, accessToken: null, refreshToken: null, isAuthenticated: false };
    }
  }

  // Декодируем JWT без библиотек — берём payload из base64
  private decodeUser(token: string): User {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return {
        email: payload.email ?? payload.sub ?? '',
        userName: payload.name ?? payload.given_name ?? undefined,
      };
    } catch {
      return { email: '', userName: '' };
    }
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // exp в секундах, Date.now() в миллисекундах
      return payload.exp * 1000 < Date.now();
    } catch {
      return true;
    }
  }
}
