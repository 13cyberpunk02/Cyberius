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

  // Проактивный refresh - токен истёк или скоро истечёт
  if (!isRetry && auth.isAccessTokenExpiredOrExpiring()) {
    // Refresh token тоже истёк - сразу разлогиниваем
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
      catchError((refreshErr: HttpErrorResponse) => {
        // Refresh вернул 401/403 - значит токен инвалидирован на сервере
        // (например зашли с другого устройства и там перевыпустили токены)
        if (refreshErr.status === 401 || refreshErr.status === 403) {
          auth.clearState();
          toast.info(getApiError(refreshErr, 'Сессия завершена. Войдите снова.'));
        }
        return throwError(() => refreshErr);
      }),
    );
  }

  const token = auth.accessToken();
  const authedReq = token ? addBearer(req, token) : req;

  return next(authedReq).pipe(
    catchError((err: HttpErrorResponse) => {
      // 401 на обычном запросе - пробуем refresh один раз
      if (err.status === 401 && !isRetry && !isAuthUrl(req.url)) {
        // Нет смысла рефрешить если refresh token истёк
        if (auth.isRefreshTokenExpired()) {
          auth.clearState();
          return throwError(() => err);
        }

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
          catchError((refreshErr: HttpErrorResponse) => {
            // Refresh не удался - сессия мертва, разлогиниваем
            if (refreshErr.status === 401 || refreshErr.status === 403) {
              auth.clearState();
              toast.info('Сессия завершена. Войдите снова.');
            }
            return throwError(() => refreshErr);
          }),
        );
      }

      // Глобальный toast для остальных ошибок
      if (!isSilentUrl(req.url)) {
        if (err.status === 403) {
          toast.error(getApiError(err, 'Доступ запрещён'));
        } else if (err.status === 404) {
          // 404 - компонент сам обработает
        } else if (err.status >= 500) {
          toast.error('Ошибка сервера. Попробуйте позже');
        } else if (err.status !== 401) {
          toast.error(getApiError(err));
        }
      }

      return throwError(() => err);
    }),
  );
};
