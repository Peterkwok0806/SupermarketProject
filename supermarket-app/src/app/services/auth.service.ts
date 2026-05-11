import { Injectable, inject, signal, computed } from '@angular/core';
import { firstValueFrom, lastValueFrom } from 'rxjs';
import { RegisterRequest, AuthResponse, LoginRequest } from '../models/auth';
import { AuthApiService } from './auth-api.service';
import { Router } from '@angular/router';
import { CartService } from './cart.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private authApi = inject(AuthApiService);
  private router = inject(Router);
  private cartService = inject(CartService);
  currentUser = signal<any>(null);
  isLoggedIn = signal<boolean>(false);
  isLoading = signal<Boolean>(false);

  constructor() { 
    this.loadTokenFromStorage();
  }

  async registerUser(data: RegisterRequest){
    console.log('1. 開始執行');
    this.isLoading.set(true);
    console.log('2. 準備發送 API...');
    try{
      const response = await lastValueFrom(this.authApi.register(data));
      console.log('3. API 回傳結果:', response);
      if (response.success){
        alert("🎉 Registration successful!");
        this.router.navigate(['/login']);
        return response;
      }else{
        throw new Error(response.message || 'Registration failed');
      }
    }catch (error) {
    console.error('registrated failed', error);
    throw error;
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

        await this.cartService.loadCart(); 
        return true;
      }return false;
    }catch (error: any){
      console.error('Login error', error);
      throw new Error(error.error?.message || '登入失敗');
    }finally{
      this.isLoading.set(false);
    }
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    this.currentUser.set(null);
    this.isLoggedIn.set(false);
    this.cartService.resetCart();
    this.router.navigate(['/login']);
  }

  isTokenValid(): boolean {
    return this.isLoggedIn();
  }


}
