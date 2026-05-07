import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RegisterRequest, AuthResponse } from '../models/auth';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {

  private apiUrl = 'https://localhost:7154/api/auth';

  constructor(private http: HttpClient) { }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data);
  }
}
