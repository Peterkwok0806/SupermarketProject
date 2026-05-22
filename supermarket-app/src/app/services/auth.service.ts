import { Injectable, inject, signal, computed } from '@angular/core';
import { lastValueFrom, Observable, tap } from 'rxjs';
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
 
  currentUser = signal<any>(null);
  isLoggedIn = signal<boolean>(false);
  isLoading = signal<Boolean>(false);

  constructor() { 
    this.loadTokenFromStorage();
  }

  // 獲取目前 LocalStorage 中的 Access Token，提供給全域攔截器組裝 Header
  getAccessToken(): string | null {
    return localStorage.getItem('token');
  }

  // 💡 全新新增：提供給全域攔截器（authInterceptor）使用的無感刷新方法
  // 這裡回傳 Observable 是為了配合攔截器的 RxJS 管道操作 (switchMap/catchError)
  refreshToken(): Observable<AuthResponse> {
    return this.authApi.refreshToken().pipe(
      tap({
        next: (response) => {
          if (response?.success && response.token) {
            // 收到新發放的 Access Token，更新本地存儲與狀態
            localStorage.setItem('token', response.token);
            if (response.userdto) {
              localStorage.setItem('currentUser', JSON.stringify(response.userdto));
              this.currentUser.set(response.userdto);
            }
            this.isLoggedIn.set(true);
          }
        },
        error: (err) => {
          console.error('背景自動刷新憑證失敗，Refresh Token 已過期', err);
          this.clearLocalData(); // 刷新失敗代表沒救了，直接抹除本地殘留資料
        }
      })
    );
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

  
  // 💡 修正優化：將原本的同步登出改為 async 非同步
  // 除了清除前端記憶體，還必須發送請求通知後端 .NET 刪除 Cookie 裡的 Refresh Token
  async logout() {
    try {
      // 💡 呼叫後端 API，讓後端將過期的空 Cookie 覆蓋回來進而銷毀憑證
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
  }

  isTokenValid(): boolean {
    return this.isLoggedIn();
  }


}
