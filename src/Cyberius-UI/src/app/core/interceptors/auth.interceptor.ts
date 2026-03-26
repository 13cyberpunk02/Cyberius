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
      if (err.status !== 401 || !auth.isAuthenticated() || isAuthUrl(req.url)) {
        return throwError(() => err);
      }

      if (auth.isRefreshTokenExpired()) {
        auth.logout();
        return throwError(() => err);
      }

      return auth.refresh().pipe(
        switchMap((): Observable<HttpEvent<unknown>> => {
          const newToken = auth.accessToken();
          const retryReq = newToken ? addBearer(req, newToken) : req;
          return next(retryReq);
        }),
        catchError((refreshErr) => {
          auth.logout();
          return throwError(() => refreshErr);
        }),
      );
    }),
  );
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  if (isAuthUrl(req.url)) return next(req);
  if (!auth.isAuthenticated()) return next(req);

  // Проактивное обновление — токен истёк или истекает через <30 сек
  if (auth.isAccessTokenExpiredOrExpiring()) {
    if (auth.isRefreshTokenExpired()) {
      auth.logout();
      return next(req);
    }

    return auth.refresh().pipe(
      switchMap((): Observable<HttpEvent<unknown>> => sendWithToken(req, next, auth)),
      catchError((err) => {
        auth.logout();
        return throwError(() => err);
      }),
    );
  }

  return sendWithToken(req, next, auth);
};
