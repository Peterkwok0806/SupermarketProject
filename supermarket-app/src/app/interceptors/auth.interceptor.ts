import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError, BehaviorSubject, filter, take, switchMap } from 'rxjs';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;
const refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  
  const authService = inject(AuthService);
  const token = authService.getAccessToken();
  
  let authReq = req;
  
  if (token && req.url.includes('localhost:7154')) {
    authReq = req.clone({
      setHeaders: {Authorization: `Bearer ${token}`}
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // 💡 2. 當後端回傳 401（Access Token 過期或無效），啟動無感刷新機制
      if (error.status === 401 && req.url.includes('localhost:7154')) {
        
        // 💡 情境 A：目前還沒有人在更新 Token，由本請求發起更新
        if (!isRefreshing) {
          isRefreshing = true;
          refreshTokenSubject.next(null); // 清空舊的排隊訊號

          console.log('🕒 偵測到 Access Token 過期，開始進行背景無感刷新...');

          // 呼叫 AuthService 的刷新機制
          return authService.refreshToken().pipe(
            switchMap((response) => {
              isRefreshing = false;
              
              // 取得新換回來的 Access Token
              const newToken = response.token ?? '';
              refreshTokenSubject.next(newToken); // 通知後續正在排隊的 API 放行

              console.log('✨ 憑證刷新成功！自動重試原先失敗的 API 請求。');

              // 💡 核心：用最新的 Token 複製一個新請求，並重新發送
              return next(req.clone({
                setHeaders: { Authorization: `Bearer ${newToken}` }
              }));
            }),
            catchError((refreshError) => {
              // ❌ 連 Refresh Token 都過期了（例如使用者 7 天沒開網頁），徹底沒救
              isRefreshing = false;
              console.error('❌ Refresh Token 已過期或失效，引導用戶強制登出。');
              
              authService.logout(); // 呼叫你的 async logout 清理 Cookie 與 LocalStorage 并跳轉
              return throwError(() => refreshError);
            })
          );
        } 
        
        // 💡 情境 B：目前正有其他 API 在發起刷新，本請求直接進入「排隊等待」
        else {
          console.warn('⏳ 憑證更新中，此 API 請求進入排隊隊列...');
          return refreshTokenSubject.pipe(
            filter(token => token !== null), // 過濾掉初始的 null，等到新 Token 出爐為止
            take(1),                         // 只取一次新 Token 就斷開訂閱
            switchMap((newJwt) => {
              // 拿到排隊出爐的新 Token，自動帶上並重新發送
              return next(req.clone({
                setHeaders: { Authorization: `Bearer ${newJwt}` }
              }));
            })
          );
        }
      }
      // 如果是 400, 403, 404, 500 等其他錯誤，不處理直接往外拋
      return throwError(() => error);
    })
  );
};
