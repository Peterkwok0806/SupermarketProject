import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError} from 'rxjs';
import { AuthService } from '../services/auth.service';


export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  // 如果請求標記要跳過攔截器（例如 refreshToken），直接放行
  if (req.headers.has('X-Skip-Interceptor')) {
    const skippedReq = req.clone({
      headers: req.headers.delete('X-Skip-Interceptor')
    });
    return next(skippedReq);
  }

  const token = authService.getAccessToken();
  let authReq = req;

  // 只對後端 API 加上 Token
  if (token && req.url.includes('localhost:7154')) {
    authReq = req.clone({
      setHeaders: {Authorization: `Bearer ${token}`}
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) =>{
      if (error.status === 401 && req.url.includes('localhost:7154')){
        return authService.handle401Error(req, next);
      }
      return throwError(() => error);
    })
    );
};

  