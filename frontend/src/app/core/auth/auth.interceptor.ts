import { HttpErrorResponse, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { API_BASE } from '../api.config';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);

  // Only our own API gets the token, and the login/refresh/logout calls never need it.
  if (!request.url.startsWith(API_BASE) || request.url.startsWith(`${API_BASE}/auth/`)) {
    return next(request);
  }

  return sendWithToken(request, next, auth.getAccessToken()).pipe(
    catchError((error: unknown) => {
      const expired = error instanceof HttpErrorResponse && error.status === 401 && auth.hasRefreshToken();
      if (!expired) {
        return throwError(() => error);
      }

      return auth.refreshAccessToken().pipe(
        switchMap((token) => sendWithToken(request, next, token)),
        catchError((refreshError: unknown) => {
          auth.logout();
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};

function sendWithToken(request: HttpRequest<unknown>, next: HttpHandlerFn, token: string | null) {
  if (!token) {
    return next(request);
  }
  return next(request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
}
