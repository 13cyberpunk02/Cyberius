import {
  HttpInterceptorFn,
  HttpErrorResponse,
  HttpRequest,
  HttpContext,
  HttpContextToken,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';
import { getApiError } from '../helpers/api-error.helper';

const IS_RETRY = new HttpContextToken<boolean>(() => false);

const SILENT_URLS = [
  '/auth/login',
  '/auth/register',
  '/auth/refresh-token',
  '/auth/forgot-password',
  '/auth/reset-password',
  '/auth/confirm-email',
  '/newsletter/subscribe',
];

function isAuthUrl(url: string): boolean {
  return ['/auth/login', '/auth/register', '/auth/refresh-token'].some((p) => url.includes(p));
}

function isSilentUrl(url: string): boolean {
  return SILENT_URLS.some((p) => url.includes(p));
}

function addBearer(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const toast = inject(ToastService);

  if (isAuthUrl(req.url)) return next(req);
  if (!auth.isAuthenticated()) return next(req);

  const isRetry = req.context.get(IS_RETRY);

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
      catchError((err) => throwError(() => err)),
    );
  }

  const token = auth.accessToken();
  const authedReq = token ? addBearer(req, token) : req;

  return next(authedReq).pipe(
    catchError((err: HttpErrorResponse) => {
      // 401 — пробуем refresh
      if (err.status === 401 && !isRetry && !isAuthUrl(req.url) && !auth.isRefreshTokenExpired()) {
        return auth.refresh().pipe(
          switchMap(() => {
            const newToken = auth.accessToken();
            const retryReq = newToken
              ? addBearer(req, newToken).clone({
                  context: new HttpContext().set(IS_RETRY, true),
                })
              : req;
            return next(retryReq);
          }),
          catchError((refreshErr) => throwError(() => refreshErr)),
        );
      }

      // Глобальный toast для всех ошибок кроме silent URLs
      if (!isSilentUrl(req.url)) {
        const message = getApiError(err);

        if (err.status === 403) {
          toast.error('Доступ запрещён');
        } else if (err.status === 404) {
          // 404 обычно не показываем — компонент сам обработает
        } else if (err.status >= 500) {
          toast.error('Ошибка сервера. Попробуйте позже');
        } else if (err.status !== 401) {
          toast.error(message);
        }
      }

      return throwError(() => err);
    }),
  );
};
