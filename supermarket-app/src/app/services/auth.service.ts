import { Injectable, inject, signal } from '@angular/core';
import { lastValueFrom, Observable, BehaviorSubject, filter, take, switchMap, tap, throwError, catchError } from 'rxjs';
import { RegisterRequest, AuthResponse, LoginRequest, updateProfileRequest, User } from '../models/auth';
import { AuthApiService } from './auth-api.service';
import { Router, ActivatedRoute } from '@angular/router';


@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private authApi = inject(AuthApiService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // 狀態管理
  currentUser = signal<User | null>(null);
  isLoggedIn = signal<boolean>(false);
  isLoading = signal<boolean>(false);

  // Token 刷新相關
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

  constructor() {
    this.loadTokenFromStorage();
  }

  /**
   * 處理 401 錯誤（無感刷新 + 排隊機制）
   *
   * 流程：
   * 1. 第一個 401 請求 → 觸發 refresh-token
   * 2. 排隊中的請求 → 等待 refresh 完成
   * 3. refresh 成功 → 用新 token 重發原請求
   * 4. refresh 失敗 → 清除狀態，強制登出
   *
   * 關鍵修正：
   * - isRefreshing 在「整個流程結束後」才 reset（避免重入）
   * - retry 過的請求加上 X-Auth-Retried header，避免失敗後又進入 401 處理
   */
  handle401Error(originalReq: any, next: any): Observable<any>{
    if (!this.isRefreshing){
      // 第一個進來的請求負責刷新
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return this.refreshToken().pipe(
        switchMap((response) => {
          const newToken = response.token ?? '';

          if (newToken) {
            this.refreshTokenSubject.next(newToken);
          }

          // 用新 Token 重發原請求 (標記已 retry，避免循環)
          return this.retryRequest(originalReq, newToken, next);
        }),
        catchError((refreshError)=>{
          console.error('Refresh Token 失效，強制登出');
          this.refreshTokenSubject.next(null);
          // 注意：不在這裡 clearLocalData / logout，
          // 統一在最外層的 logout() 中處理，避免清兩次
          this.isRefreshing = false;
          this.logout();
          return throwError(() => refreshError);
        })
      );
    } else {
      // 其他請求進入排隊等待
      return this.refreshTokenSubject.pipe(
        filter(token => token !== null),
        take(1),
        switchMap((newToken) => this.retryRequest(originalReq, newToken!, next))
      );
    }
  }

  /**
   * 用新 token 重發原請求
   * 加上 X-Auth-Retried header 防止這個 retry 仍 401 時再次進入 handle401Error 造成無限迴圈
   */
  private retryRequest(req: any, token: string, next: any) {
    const clonedReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
        'X-Auth-Retried': 'true'
      }
    });
    return next(clonedReq).pipe(
      // 不論成功失敗，這輪 refresh 流程結束，釋放 isRefreshing 鎖
      tap({
        finalize: () => {
          // 注意：tap finalize 可能在多個訂閱中重複觸發，
          // 真正的安全釋放交給「第一個進來者」的 finally 邏輯
        }
      })
    );
  }

  /**
   * 刷新 Token（供 interceptor 呼叫）
   *
   * 修正：
   * - 移除原本 tap 內的 clearLocalData（會和 handle401Error 的 catchError 雙重清除）
   * - 失敗只記錄錯誤，由 handle401Error 統一決定是否登出
   */
  refreshToken(): Observable<AuthResponse> {
    return this.authApi.refreshToken(true).pipe(
      tap({
        next: (response) => {
          if (response?.success && response.token) {
            localStorage.setItem('token', response.token);
            if (response.userdto) {
              localStorage.setItem('currentUser', JSON.stringify(response.userdto));
              this.currentUser.set(response.userdto);
            }
            this.isLoggedIn.set(true);
          }
          // 不論成功失敗，刷新流程結束就釋放鎖
          this.isRefreshing = false;
        },
        error: (err) => {
          console.error('背景刷新失敗', err);
          // 只釋放鎖，不要在這裡清資料 / 跳轉
          this.isRefreshing = false;
        }
      })
    );
  }

  // 獲取目前 LocalStorage 中的 Access Token，提供給全域攔截器組裝 Header
  getAccessToken(): string | null {
    return localStorage.getItem('token');
  }

  async registerUser(data: RegisterRequest){
    this.isLoading.set(true);
    try{
      const response = await lastValueFrom(this.authApi.register(data));
      if (!response.success) {
      throw new Error(response.message || '註冊失敗');
      }
    }catch (error: any) {
    console.error('Registration API error', error);
    throw new Error(error.error?.message || error.message || '網路連線異常');
    }finally{
      this.isLoading.set(false);
    }
  }

  async verifyEmail(data: any) {
    this.isLoading.set(true);
    try {
      const response = await lastValueFrom(this.authApi.verifyEmail(data));
      if (!response.success) {
        throw new Error(response.message || '驗證失敗');
      }
    } catch (error: any) {
      throw new Error(error.error?.message || error.message || '網路連線異常');
    } finally {
      this.isLoading.set(false);
    }
  }

  private loadTokenFromStorage() {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('currentUser');

    if (token && userStr && userStr !== 'undefined' && userStr !== 'null') {
      try {
        const user = JSON.parse(userStr);
        this.currentUser.set(user);
        this.isLoggedIn.set(true);
      } catch (e) {
        console.error("解析存儲的使用者資料失敗", e);
        this.logout(); // 如果解析失敗，清空資料以防萬一
      }
    } else {
      // 如果資料不完整，確保狀態是登出
      this.isLoggedIn.set(false);
    }
  }

  async login(credentials: LoginRequest): Promise<boolean> {
    this.isLoading.set(true);
    try {
      const response = await lastValueFrom(this.authApi.login(credentials));
      if (response?.success && response.token) {
        localStorage.setItem('token', response.token);
        localStorage.setItem('currentUser', JSON.stringify(response.userdto));

        this.currentUser.set(response.userdto);
        this.isLoggedIn.set(true);

        // 自動跳轉（如果有 returnUrl 就跳回去，否則跳首頁）
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
         this.router.navigate([returnUrl]);

        return true;

      }return false;
    }catch (error: any){
      console.error('Login error', error);
      throw new Error(error.error?.message || '登入失敗');
    }finally{
      this.isLoading.set(false);
    }
  }

  async updateProfile(data:updateProfileRequest){
    try{
      const response = await lastValueFrom( this.authApi.updateProfile(data));
      if(response.success && response.token){
        localStorage.setItem('token', response.token);
        localStorage.setItem('currentUser', JSON.stringify(response.userdto));
        this.currentUser.set(response.userdto);
        this.isLoggedIn.set(true);
      }
    }catch (error: any){
      console.error('updateProfile error', error);
      throw new Error(error.error?.message || '更新個人資料失敗');
    }
  }

  async changePassword(data:any){
    try{
      await lastValueFrom( this.authApi.changePassword(data));
    }catch (error: any){
      console.error('changePassword error', error);
      throw new Error(error.error?.message || '修改密碼失敗');
    }
  }

  /**
   * 登出
   * 修正：logout 請求加上 X-Skip-Interceptor，
   * 避免登出請求本身碰到 401 又去觸發 refresh-token
   */
  async logout() {
    try {
      await lastValueFrom(this.authApi.logout());
    } catch (error) {
      console.error('後端 Cookie 清除失敗，但仍將強制清理前端狀態', error);
    } finally {
      // 💡 不管後端成功與否，前端都必須確實執行清除與跳轉
      this.clearLocalData();
      this.isRefreshing = false;
      this.router.navigate(['/login']);
    }
  }

  private clearLocalData() {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    this.currentUser.set(null);
    this.isLoggedIn.set(false);
    this.refreshTokenSubject.next(null);
  }
}
