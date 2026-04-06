import {
  HttpInterceptorFn,
  HttpErrorResponse,
  HttpRequest,
  HttpContext,
  HttpContextToken,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError, Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

const AUTH_URLS = ['/auth/login', '/auth/register', '/auth/refresh-token'];

const IS_RETRY = new HttpContextToken<boolean>(() => false);

function isAuthUrl(url: string): boolean {
  return AUTH_URLS.some((path) => url.includes(path));
}

function addBearer(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  // Auth endpoints — без токена
  if (isAuthUrl(req.url)) return next(req);

  // Не авторизован — просто шлём без токена
  if (!auth.isAuthenticated()) return next(req);

  // Уже повторный запрос после refresh — не делаем ещё один refresh
  const isRetry = req.context.get(IS_RETRY);

  // Access token истёк/истекает — нужен проактивный refresh
  if (!isRetry && auth.isAccessTokenExpiredOrExpiring()) {
    if (auth.isRefreshTokenExpired()) {
      auth.clearState();
      return next(req);
    }

    return auth.refresh().pipe(
      switchMap(() => {
        const token = auth.accessToken();
        const retryReq = token
          ? addBearer(req, token).clone({
              context: new HttpContext().set(IS_RETRY, true),
            })
          : req;
        return next(retryReq);
      }),
      catchError((err) => {
        if (err?.status !== 0) {
          // Сетевые ошибки не разлогиниваем — refresh() сам обработает 401/403
        }
        return throwError(() => err);
      }),
    );
  }

  // Токен валиден — отправляем с токеном
  const token = auth.accessToken();
  const authedReq = token ? addBearer(req, token) : req;

  return next(authedReq).pipe(
    catchError((err: HttpErrorResponse) => {
      // Не 401, не авторизован, auth endpoint, или уже retry — пробрасываем
      if (err.status !== 401 || !auth.isAuthenticated() || isAuthUrl(req.url) || isRetry) {
        return throwError(() => err);
      }

      // Нет refresh token — сессия мертва
      if (auth.isRefreshTokenExpired()) {
        auth.clearState();
        return throwError(() => err);
      }

      // 401 на обычном запросе — пробуем refresh ОДИН раз
      return auth.refresh().pipe(
        switchMap(() => {
          const newToken = auth.accessToken();
          // Помечаем запрос как retry чтобы не зациклиться
          const retryReq = newToken
            ? addBearer(req, newToken).clone({
                context: new HttpContext().set(IS_RETRY, true),
              })
            : req;
          return next(retryReq);
        }),
        catchError((refreshErr) => throwError(() => refreshErr)),
      );
    }),
  );
};
