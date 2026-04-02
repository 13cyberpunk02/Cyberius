import {
  HttpInterceptorFn,
  HttpErrorResponse,
  HttpRequest,
  HttpHandlerFn,
  HttpEvent,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError, Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

const AUTH_URLS = ['/auth/login', '/auth/register', '/auth/refresh-token'];

function isAuthUrl(url: string): boolean {
  return AUTH_URLS.some((path) => url.includes(path));
}

function addBearer(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

function sendWithToken(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  auth: AuthService,
): Observable<HttpEvent<unknown>> {
  const token = auth.accessToken();
  const authedReq = token ? addBearer(req, token) : req;

  return next(authedReq).pipe(
    catchError((err: HttpErrorResponse) => {
      // Не 401 или не авторизован — пробрасываем ошибку
      if (err.status !== 401 || !auth.isAuthenticated() || isAuthUrl(req.url)) {
        return throwError(() => err);
      }

      // Нет refresh token — сессия точно мертва
      if (auth.isRefreshTokenExpired()) {
        auth.clearState();
        return throwError(() => err);
      }

      // Пробуем обновить токен
      return auth.refresh().pipe(
        switchMap((): Observable<HttpEvent<unknown>> => {
          const newToken = auth.accessToken();
          const retryReq = newToken ? addBearer(req, newToken) : req;
          return next(retryReq);
        }),
        catchError((refreshErr) => {
          // refresh() сам обработает 401/403 и очистит state
          // Здесь просто пробрасываем ошибку
          return throwError(() => refreshErr);
        }),
      );
    }),
  );
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  // Auth endpoints — без токена
  if (isAuthUrl(req.url)) return next(req);

  // Не авторизован — просто шлём без токена
  if (!auth.isAuthenticated()) return next(req);

  // Access token валиден — отправляем как есть
  if (!auth.isAccessTokenExpiredOrExpiring()) {
    return sendWithToken(req, next, auth);
  }

  // Access token истёк/истекает — нет refresh token → чистим state
  if (auth.isRefreshTokenExpired()) {
    auth.clearState();
    return next(req);
  }

  // Проактивно обновляем токен
  return auth.refresh().pipe(
    switchMap((): Observable<HttpEvent<unknown>> => sendWithToken(req, next, auth)),
    catchError((err) => {
      // При сетевой ошибке (status 0) — НЕ разлогиниваем,
      // пробрасываем ошибку чтобы компонент мог обработать
      if (err?.status === 0) {
        return throwError(() => err);
      }
      // При серверной ошибке — refresh() уже очистил state если 401/403
      return throwError(() => err);
    }),
  );
};
