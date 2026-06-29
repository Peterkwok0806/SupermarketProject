import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CouponListDto,
  ValidateCouponRequestDto,
  CouponValidationResultDto,
  ApplyCouponRequestDto,
  CouponUsageDto
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
export class CouponApiService {
  private apiUrl = `${environment.apiUrl}api/coupons`;
  private http = inject(HttpClient);

  // ===== Customer Endpoints =====

  /** Get all active, valid coupons for browsing */
  getAvailableCoupons(): Observable<ApiResultPage<CouponListDto>> {
    return this.http.get<ApiResultPage<CouponListDto>>(`${this.apiUrl}/available`);
  }

  /** Validate a coupon code before applying */
  validateCoupon(request: ValidateCouponRequestDto): Observable<ApiResult<CouponValidationResultDto>> {
    return this.http.post<ApiResult<CouponValidationResultDto>>(`${this.apiUrl}/validate`, request);
  }

  /** Apply coupon to an order after creation */
  applyCoupon(request: ApplyCouponRequestDto): Observable<ApiResult<boolean>> {
    return this.http.post<ApiResult<boolean>>(`${this.apiUrl}/apply`, request);
  }

  /** Get user's coupon usage history */
  getCouponHistory(page: number = 1, pageSize: number = 20): Observable<ApiResultPage<CouponUsageDto>> {
    return this.http.get<ApiResultPage<CouponUsageDto>>(`${this.apiUrl}/usage-history?page=${page}&pageSize=${pageSize}`);
  }
}