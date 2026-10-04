import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const addToken = (request: HttpRequest<unknown>) => {
    const token = authService.getAccessToken();
    if (!token) {
      return request;
    }
    return request.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  };
  const isAuthRequest = req.url.includes('/auth/');

  return next(addToken(req)).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthRequest || !authService.isLoggedIn()) {
        return throwError(() => error);
      }

      return authService.refresh().pipe(
        switchMap(() => next(addToken(req))),
        catchError((refreshError) => {
          authService.logout();
          router.navigate(['/login']);
          return throwError(() => refreshError);
        })
      );
    })
  );
};