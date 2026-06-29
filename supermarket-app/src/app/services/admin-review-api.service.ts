import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResult, ApiResultData, ApiResultPagination } from '../models/api-result';
import { Review, ReviewDashboard, ReviewStatus } from '../models/review';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AdminReviewApiService {
  private apiUrl = `${environment.apiUrl}api/admin/reviews`;
  private http = inject(HttpClient);

  getReviews(
    page: number = 1,
    pageSize: number = 20,
    status?: ReviewStatus,
    productId?: number,
    rating?: number,
    keyword?: string,
    fromDate?: string,
    toDate?: string
  ): Observable<ApiResultPagination<Review>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (status) params = params.set('status', status);
    if (productId) params = params.set('productId', productId.toString());
    if (rating) params = params.set('rating', rating.toString());
    if (keyword) params = params.set('keyword', keyword);
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);

    return this.http.get<ApiResultPagination<Review>>(`${this.apiUrl}`, { params });
  }

  getDashboard(): Observable<ApiResultData<ReviewDashboard>> {
    return this.http.get<ApiResultData<ReviewDashboard>>(`${this.apiUrl}/dashboard`);
  }

  getReview(reviewId: number): Observable<ApiResultData<Review>> {
    return this.http.get<ApiResultData<Review>>(`${this.apiUrl}/${reviewId}`);
  }

  updateStatus(reviewId: number, status: ReviewStatus, note?: string): Observable<ApiResult> {
    return this.http.put<ApiResult>(`${this.apiUrl}/${reviewId}/status`, { status, note });
  }

  replyToReview(reviewId: number, reply: string): Observable<ApiResult> {
    return this.http.put<ApiResult>(`${this.apiUrl}/${reviewId}/reply`, { reply });
  }

  deleteReview(reviewId: number): Observable<ApiResult> {
    return this.http.delete<ApiResult>(`${this.apiUrl}/${reviewId}`);
  }
}