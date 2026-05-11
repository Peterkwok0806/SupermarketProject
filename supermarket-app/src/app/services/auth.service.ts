import { Injectable, inject, signal, computed } from '@angular/core';
import { firstValueFrom, lastValueFrom } from 'rxjs';
import { RegisterRequest, AuthResponse, LoginRequest } from '../models/auth';
import { AuthApiService } from './auth-api.service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private authApi = inject(AuthApiService);
  private router = inject(Router);
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
    const user = localStorage.getItem('currentUser');
    
    if (token && user) {
      this.currentUser.set(JSON.parse(user));
      this.isLoggedIn.set(true);
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
        localStorage.setItem('currentUser', JSON.stringify(response.user));

        this.currentUser.set(response.user);
        this.isLoggedIn.set(true);

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
    this.router.navigate(['/login']);
  }

  isTokenValid(): boolean {
    return this.isLoggedIn();
  }


}
