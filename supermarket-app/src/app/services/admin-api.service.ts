import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiResultPagination, ApiResult } from '../models/api-result';
import { AdminUser, UpdateUserStatus, UpdateUserRole } from '../models/admin-user';

@Injectable({
  providedIn: 'root'
})
export class AdminApiService {
  private apiUrl = 'https://localhost:7154/api/admin';
  private http = inject(HttpClient);

  getUsers(page: number = 1, pageSize: number = 10, search?: string): Observable<ApiResultPagination<AdminUser>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<ApiResultPagination<AdminUser>>(`${this.apiUrl}/users`, { params });
  }

  updateUserStatus(userId: number, isActive: boolean): Observable<ApiResult> {
    return this.http.put<ApiResult>(`${this.apiUrl}/users/${userId}/status`, { isActive });
  }

  updateUserRole(userId: number, role: string): Observable<ApiResult> {
    return this.http.put<ApiResult>(`${this.apiUrl}/users/${userId}/role`, { role });
  }
}