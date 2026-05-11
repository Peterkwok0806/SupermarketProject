import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  
  const injector = inject(Injector);

  const token = localStorage.getItem('token');
  let authReq = req;
  
  if (token && req.url.includes('localhost:7154')) {
    authReq = req.clone({
      setHeaders: {Authorization: `Bearer ${token}`}
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // 4. 如果後端回傳 401，代表 Token 過期或無效
      if (error.status === 401) {
        console.warn('偵測到 401 錯誤，準備強制登出...');
        const authService = injector.get(AuthService);
        authService.logout(); // 呼叫你寫好的 logout 清空資料並導向 Login
      }
      return throwError(() => error);
      })
  );
};
