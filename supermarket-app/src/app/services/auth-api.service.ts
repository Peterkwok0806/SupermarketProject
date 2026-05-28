import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RegisterRequest, AuthResponse, LoginRequest, updateProfileRequest } from '../models/auth';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {

  private apiUrl = 'https://localhost:7154/api/auth';
  private http = inject(HttpClient);

  constructor() { }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data);
  }

  login(data: LoginRequest): Observable<AuthResponse> {
  return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data, {
    withCredentials: true // 關鍵：強迫瀏覽器接收並儲存後端發回的 HttpOnly Cookie
  });
}

  refreshToken(skipInterceptor: boolean = false): Observable<AuthResponse> {
    let headers: any = {};
    if (skipInterceptor) {
    headers['X-Skip-Interceptor'] = 'true';
  }
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh-token`, {}, {
      headers,
      withCredentials: true // 關鍵：強制瀏覽器在背景自動帶上名稱為 refreshToken 的 Cookie
    });
  }

  logout(): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/logout`, {}, {
      withCredentials: true // 關鍵：讓後端有權限對這個跨網域請求覆蓋空 Cookie
    });
  }

  updateProfile(data:updateProfileRequest):Observable<AuthResponse>{
    return this.http.put<AuthResponse>(`${this.apiUrl}/profile`, data);
  }

  changePassword(data: any): Observable<AuthResponse> {
  return this.http.put<any>(`${this.apiUrl}/change-password`, data);
}
}
