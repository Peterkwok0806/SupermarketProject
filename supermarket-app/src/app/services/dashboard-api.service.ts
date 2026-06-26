import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResultData } from '../models/api-result';
import { DashboardStats, SalesTrend } from '../models/dashboard';

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

  /**
   * 取得最近 N 天的每日銷售趨勢 (後端會補齊零銷量日)
   * @param days 查詢天數，預設 7
   */
  getSalesTrend(days: number = 7): Observable<SalesTrend> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.get<ApiResultData<SalesTrend>>(`${this.apiUrl}/sales-trend`, { params }).pipe(
      map(res => res.item!)
    );
  }
}