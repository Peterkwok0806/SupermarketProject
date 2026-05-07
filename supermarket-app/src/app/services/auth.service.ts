import { Injectable, inject, signal, computed } from '@angular/core';
import { firstValueFrom, lastValueFrom } from 'rxjs';
import { RegisterRequest, AuthResponse } from '../models/auth';
import { AuthApiService } from './auth-api.service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private authApi = inject(AuthApiService);
  private router = inject(Router);
  currentUser = signal<any>(null);
  isLoading = signal<Boolean>(false);

  constructor() { }

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
}
