import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An error occurred';

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Error: ${error.error.message}`;
      } else {
        // Server-side error
        // First check if it's our API response format with a message
        if (error.error?.message) {
          errorMessage = error.error.message;
        } else {
          switch (error.status) {
            case 401:
              // Unauthorized - redirect to login
              router.navigate(['/login']);
              errorMessage = 'Unauthorized. Please login again.';
              break;
            case 403:
              errorMessage = 'You do not have permission to access this resource.';
              break;
            case 404:
              errorMessage = 'Resource not found.';
              break;
            case 500:
              errorMessage = 'Internal server error. Please try again later.';
              break;
            default:
              errorMessage = error.error?.error || error.message || 'Something went wrong';
          }
        }
      }

      console.error('HTTP Error:', errorMessage, error);
      return throwError(() => new Error(errorMessage));
    })
  );
};
