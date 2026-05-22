import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const guestGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    return true; // 💡 沒登入，放行去登入/註冊頁面
  } else {
    // 💡 已登入，直接彈回首頁
    router.navigate(['/']);
    return false;
  }
};
