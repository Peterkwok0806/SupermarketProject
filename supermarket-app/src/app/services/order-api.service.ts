import { Injectable, inject } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OrderApiService {
  private apiUrl = 'https://localhost:7154/api/order';

  private http = inject(HttpClient);

  constructor() { }

  /** 建立訂單 */
  createOrder(orderData: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, orderData);
  }

  /** 取得單筆訂單 */
  getOrderById(orderId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${orderId}`);
  }

  /** 取得我的所有訂單 */
  getMyOrders(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }
}
