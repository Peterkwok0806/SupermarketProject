import { Injectable, inject } from '@angular/core';
import { HttpClient,HttpParams} from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResult,ApiResultPagination} from '../models/api-result';
import { OrderEntity,OrderStatus,searchOrderRequest } from '../models/order';


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
  getOrderById(ordersnowflakeId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${ordersnowflakeId}`);
  }

  /** 取得我的所有訂單 */
  getMyOrders(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  searchOrders(req?: searchOrderRequest, page: number = 1, pageSize: number = 10): Observable<ApiResultPagination<OrderEntity>> {
    let params = new HttpParams()
    .set('page', page.toString())
    .set('pageSize', pageSize.toString());

    // 動態加入可選的查詢條件
    if(req!=null){
      if (req.orderId) params = params.set('snowflakeId', req.orderId.toString());
      if (req.userName) params = params.set('userName', req.userName.toString());
      if (req.startDate) params = params.set('startDate', req.startDate.toISOString().split('T')[0]);
      if (req.endDate) params = params.set('endDate', req.endDate.toISOString().split('T')[0]);
    }
    
    return this.http.get<ApiResultPagination<OrderEntity>>(`${this.apiUrl}/search`, {params});
  }

  updateStatus(ordersnowflakeId: string,newStatus: OrderStatus):Observable<ApiResult>{
    return this.http.put<ApiResult>(`${this.apiUrl}/${ordersnowflakeId}/status`, newStatus)
  }

}
