import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from './auth.service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const allowedRoles = route.data['roles'] as string[];
  
  if (!authService.isAuthenticated) {
    router.navigate(['/login']);
    return false;
  }

  if (allowedRoles && !authService.hasAnyRole(allowedRoles)) {
    router.navigate(['/unauthorized']);
    return false;
  }

  return true;
};
