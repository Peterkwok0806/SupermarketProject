import { CanActivateFn, Router} from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.currentUser();

  if (user && user.role === 'Admin') {
    return true;
  } else {
    router.navigate(['/']);
    return false;
  }
};
