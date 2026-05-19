import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RegisterRequest, AuthResponse, LoginRequest, updateProfileRequest } from '../models/auth';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {

  private apiUrl = 'https://localhost:7154/api/auth';

  constructor(private http: HttpClient) { }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data);
  }

  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data);
  }

  updateProfile(data:updateProfileRequest):Observable<AuthResponse>{
    return this.http.put<AuthResponse>(`${this.apiUrl}/profile`, data);
  }

  changePassword(data: any): Observable<AuthResponse> {
  return this.http.put<any>(`${this.apiUrl}/change-password`, data);
}
}
