import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CouponListDto,
  CouponStatsDto,
  CouponType,
  CreateCouponDto,
  UpdateCouponDto
} from '../models/coupon';
import { environment } from '../../environments/environment';

export interface ApiResult<T> {
  success: boolean;
  message?: string;
  item?: T;
}

export interface ApiResultPage<T> {
  success: boolean;
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class AdminCouponApiService {
  private apiUrl = `${environment.apiUrl}api/admin/coupons`;
  private http = inject(HttpClient);

  /** Get coupon dashboard statistics */
  getStats(): Observable<ApiResult<CouponStatsDto>> {
    return this.http.get<ApiResult<CouponStatsDto>>(`${this.apiUrl}/stats`);
  }

  /** Get paginated coupon list with filters */
  getCoupons(
    page: number = 1,
    pageSize: number = 20,
    search?: string,
    type?: CouponType,
    isActive?: boolean,
    isExpired?: boolean,
    sort?: string
  ): Observable<ApiResultPage<CouponListDto>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) params = params.set('search', search);
    if (type !== undefined && type !== null) params = params.set('type', type.toString());
    if (isActive !== undefined && isActive !== null) params = params.set('isActive', isActive.toString());
    if (isExpired !== undefined && isExpired !== null) params = params.set('isExpired', isExpired.toString());
    if (sort) params = params.set('sort', sort);

    return this.http.get<ApiResultPage<CouponListDto>>(this.apiUrl, { params });
  }

  /** Get single coupon by ID */
  getCoupon(id: number): Observable<ApiResult<CouponListDto>> {
    return this.http.get<ApiResult<CouponListDto>>(`${this.apiUrl}/${id}`);
  }

  /** Create a new coupon */
  createCoupon(dto: CreateCouponDto): Observable<ApiResult<CouponListDto>> {
    return this.http.post<ApiResult<CouponListDto>>(this.apiUrl, dto);
  }

  /** Update an existing coupon */
  updateCoupon(dto: UpdateCouponDto): Observable<ApiResult<CouponListDto>> {
    return this.http.put<ApiResult<CouponListDto>>(this.apiUrl, dto);
  }

  /** Delete a coupon */
  deleteCoupon(id: number): Observable<ApiResult<boolean>> {
    return this.http.delete<ApiResult<boolean>>(`${this.apiUrl}/${id}`);
  }

  /** Toggle coupon active/inactive status */
  toggleActive(id: number): Observable<ApiResult<boolean>> {
    return this.http.patch<ApiResult<boolean>>(`${this.apiUrl}/${id}/toggle`, {});
  }

  /** Bulk delete coupons */
  bulkDelete(ids: number[]): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/bulk-delete`, ids);
  }
}
