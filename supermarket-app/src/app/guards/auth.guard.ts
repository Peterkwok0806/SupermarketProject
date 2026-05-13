import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
  return true;
}else{
  // 未登入 → 跳到 Login 並記住原本想去的頁面
  router.navigate(['/login'], { 
      queryParams: { returnUrl: state.url } 
    });
    return false;
}
};
