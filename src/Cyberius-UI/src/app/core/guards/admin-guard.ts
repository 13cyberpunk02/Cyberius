import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { inject } from '@angular/core';

export const adminGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    router.navigate(['/']);
    return false;
  }

  const roles = auth.profile()?.roles ?? [];
  const hasAccess = roles.some((r) => r === 'Admin' || r === 'Manager');

  if (!hasAccess) {
    router.navigate(['/']);
    return false;
  }

  return true;
};
