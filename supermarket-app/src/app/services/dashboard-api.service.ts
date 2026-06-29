import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResultData } from '../models/api-result';
import { DashboardStats, SalesTrend, TopSellingProduct } from '../models/dashboard';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DashboardApiService {
  private apiUrl = `${environment.apiUrl}api/dashboard`;

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

  /**
   * 取得銷售數量最高的前 10 名商品
   */
  getTopSellingProducts(): Observable<TopSellingProduct[]> {
    return this.http.get<ApiResultData<TopSellingProduct[]>>(`${this.apiUrl}/top-selling-products`).pipe(
      map(res => res.item!)
    );
  }
}
