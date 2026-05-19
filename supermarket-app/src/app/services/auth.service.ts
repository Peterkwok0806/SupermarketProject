import { Injectable, inject, signal, computed } from '@angular/core';
import { firstValueFrom, lastValueFrom } from 'rxjs';
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
      if(response.success && response.token)
        localStorage.setItem('token', response.token);
        localStorage.setItem('currentUser', JSON.stringify(response.userdto));
        this.currentUser.set(response.userdto);
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

  
  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    this.currentUser.set(null);
    this.isLoggedIn.set(false);
  
    this.router.navigate(['/login']);
  }

  isTokenValid(): boolean {
    return this.isLoggedIn();
  }


}
