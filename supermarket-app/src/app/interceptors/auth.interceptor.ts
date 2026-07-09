import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { environment } from '../../environments/environment';


/**
 * 判斷是否為「需要帶 Authorization header」的後端 API 請求。
 * 用 environment.apiUrl 取代硬編字串，避免部署後 URL 變動導致 token 沒被加上去。
 * 注意：AI Chat API 不需要認證，排除這些端點避免 token 過期觸發登出。
 */
function isBackendApi(url: string): boolean {
  if (!url) return false;
  // 允許 http/https，忽略尾斜線
  const apiBase = environment.apiUrl.replace(/\/+$/, '');
  const isApi = url.startsWith(apiBase) || url.includes('localhost:7154');
  
  // AI Chat API: POST /api/chat 不需要認證，但 GET /api/chat/sessions 需要
  // 因此不移除 token，讓後端決定是否需要認證
  return isApi;
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  // 1) refresh-token / logout 請求：明確跳過攔截器（避免循環刷新）
  if (req.headers.has('X-Skip-Interceptor')) {
    const skippedReq = req.clone({
      headers: req.headers.delete('X-Skip-Interceptor')
    });
    return next(skippedReq);
  }

  const token = authService.getAccessToken();
  let authReq = req;

  // 2) 對後端 API 自動附上 Bearer token
  if (token && isBackendApi(req.url)) {
    authReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
        // 3) 只對「後端 API + 401 + 還沒 retry 過」觸發刷新
        // AI Chat API 的 401 是業務邏輯（「未登入」），不應觸發 token 刷新和登出
        const isAlreadyRetried = req.headers.has('X-Auth-Retried');
        if (
          error.status === 401 &&
          isBackendApi(req.url) &&
          !isAlreadyRetried &&
          !req.url.includes('/api/chat')
        ) {
          return authService.handle401Error(req, next);
        }
      return throwError(() => error);
    })
  );
};