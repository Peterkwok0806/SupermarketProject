import { Injectable, inject, signal} from '@angular/core';
import { lastValueFrom, Observable, BehaviorSubject, filter, take, switchMap, tap, throwError, catchError, firstValueFrom } from 'rxjs';
import { RegisterRequest, AuthResponse, LoginRequest, updateProfileRequest } from '../models/auth';
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
  currentUser = signal<any>(null);
  isLoggedIn = signal<boolean>(false);
  isLoading = signal<Boolean>(false);

  // Token 刷新相關
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

  constructor() { 
    this.loadTokenFromStorage();
  }

  //處理 401 錯誤（無感刷新 + 排隊機制）
  handle401Error(originalReq: any, next: any): Observable<any>{
    if (!this.isRefreshing){
      // 第一個進來的請求負責刷新
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      console.log('🕒 開始背景刷新 Access Token...');

      return this.refreshToken().pipe(
        switchMap((response) =>{
          this.isRefreshing = false;
          const newToken = response.token ?? '';

          if (newToken){
            this.refreshTokenSubject.next(newToken);
            console.log('Token 刷新成功');
          }
          // 用新 Token 重發原請求
          return this.retryRequest(originalReq, newToken, next);
        }),
        catchError((refreshError)=>{
          this.isRefreshing = false;
          this.refreshTokenSubject.next(null);
          console.error('Refresh Token 失效，強制登出');

          this.logout();
          return throwError(() => refreshError);
        })
      );
    }else{
      // 其他請求進入排隊等待
      console.warn(' Token 刷新中，請求進入等待隊列...');
      return this.refreshTokenSubject.pipe(
        filter(token => token !== null),
        take(1),
        switchMap((newToken) => this.retryRequest(originalReq, newToken!, next))
      );
    }
  }

 private retryRequest(req: any, token: string, next: any) {
    const clonedReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
    return next(clonedReq);
  }

  // 刷新 Token（供 interceptor 呼叫）
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
        },
        error: (err) => {
          console.error('背景刷新失敗', err);
          this.clearLocalData(); // 刷新失敗，直接抹除本地殘留資料
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

  async verifyEmail(data: any){
    try {
      const response = await firstValueFrom(this.authApi.verifyEmail(data));
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

    console.log('1. 開始執行');
    try{
      console.log('2. 準備發送 API...');
      const response = await lastValueFrom( this.authApi.login(credentials));
      console.log('3. API 回傳結果:', response);
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
      }
    }catch (error: any){
      console.error('Login error', error);
    }
  } 

  async changePassword(data:any){
    try{
      await lastValueFrom( this.authApi.changePassword(data));
    }catch (error: any){
      console.error('Login error', error);
    }
  }

  

  async logout() {
    try {
      await lastValueFrom(this.authApi.logout());
    } catch (error) {
      console.error('後端 Cookie 清除失敗，但仍將強制清理前端狀態', error);
    } finally {
      // 💡 不管後端成功與否，前端都必須確實執行清除與跳轉
      this.clearLocalData();
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

  isTokenValid(): boolean {
    return this.isLoggedIn();
  }


}
