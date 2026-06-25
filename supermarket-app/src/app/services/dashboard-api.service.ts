import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResultData } from '../models/api-result';
import { DashboardStats } from '../models/dashboard';

@Injectable({
  providedIn: 'root'
})
export class DashboardApiService {
  private apiUrl = 'https://localhost:7154/api/dashboard';

  private http = inject(HttpClient);

  constructor() { }

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<ApiResultData<DashboardStats>>(this.apiUrl).pipe(
      map(res => res.item!)
    );
  }
}