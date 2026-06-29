import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResult, ApiResultData, ApiResultPagination } from '../models/api-result';
import { Review, ReviewStats, ReviewImage, CreateReview, UpdateReview, CanReviewResult } from '../models/review';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReviewApiService {
  private apiUrl = `${environment.apiUrl}api/Review`;
  private http = inject(HttpClient);

  /**
   * Transform backend ReviewDto to frontend Review model.
   * Backend: imageUrls: string[], hasHelpful: boolean, adminReplyAt
   * Frontend: images: ReviewImage[], hasUserLiked: boolean, adminReplyDate, canEdit
   */
  private mapReview(backendReview: any): Review {
    const editWindowDays = 7;
    const createdAt = new Date(backendReview.createdAt);
    const canEdit = backendReview.status !== 'Rejected'
      && (Date.now() - createdAt.getTime()) < editWindowDays * 24 * 60 * 60 * 1000;

    return {
      id: backendReview.id,
      userId: backendReview.userId,
      userName: backendReview.userName,
      productId: backendReview.productId,
      productName: backendReview.productName,
      rating: backendReview.rating,
      title: backendReview.title,
      content: backendReview.content,
      isVerifiedPurchase: backendReview.isVerifiedPurchase,
      helpfulCount: backendReview.helpfulCount,
      status: backendReview.status,
      adminReply: backendReview.adminReply,
      adminReplyDate: backendReview.adminReplyAt,
      createdAt: backendReview.createdAt,
      updatedAt: backendReview.updatedAt,
      canEdit: canEdit,
      hasUserLiked: backendReview.hasHelpful ?? false,
      images: (backendReview.imageUrls || []).map((url: string, idx: number) => ({
        id: idx,
        imageUrl: url,
        sortOrder: idx
      } as ReviewImage))
    };
  }

  getProductReviews(
    productId: number,
    rating?: number,
    hasImage?: boolean,
    verifiedOnly?: boolean,
    sortBy: string = 'newest',
    page: number = 1,
    pageSize: number = 10
  ): Observable<ApiResultPagination<Review>> {
    let params = new HttpParams()
      .set('sortBy', sortBy)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (rating !== undefined && rating !== null) {
      params = params.set('rating', rating.toString());
    }
    if (hasImage !== undefined && hasImage !== null) {
      params = params.set('hasImage', hasImage.toString());
    }
    if (verifiedOnly !== undefined && verifiedOnly !== null) {
      params = params.set('verifiedOnly', verifiedOnly.toString());
    }

    return this.http.get<ApiResultPagination<any>>(`${this.apiUrl}/product/${productId}`, { params }).pipe(
      map(res => ({
        ...res,
        items: (res.items || []).map((r: any) => this.mapReview(r))
      }))
    );
  }

  /**
   * Transform backend ProductReviewStatsDto (flat counts) to frontend ReviewStats (ratingDistribution array).
   * Backend: { totalCount, averageRating, fiveStarCount, fourStarCount, ... }
   * Frontend: { totalReviews, averageRating, ratingDistribution: [{rating, count, percentage}], verifiedPurchaseCount }
   */
  getReviewStats(productId: number): Observable<ApiResultData<ReviewStats>> {
    return this.http.get<ApiResultData<any>>(`${this.apiUrl}/product/${productId}/stats`).pipe(
      map(res => {
        const s = res.item;
        const total = s.totalCount || 0;
        const distribution = [
          { rating: 5, count: s.fiveStarCount || 0, percentage: total ? Math.round((s.fiveStarCount / total) * 100) : 0 },
          { rating: 4, count: s.fourStarCount || 0, percentage: total ? Math.round((s.fourStarCount / total) * 100) : 0 },
          { rating: 3, count: s.threeStarCount || 0, percentage: total ? Math.round((s.threeStarCount / total) * 100) : 0 },
          { rating: 2, count: s.twoStarCount || 0, percentage: total ? Math.round((s.twoStarCount / total) * 100) : 0 },
          { rating: 1, count: s.oneStarCount || 0, percentage: total ? Math.round((s.oneStarCount / total) * 100) : 0 },
        ];
        return {
          ...res,
          item: {
            averageRating: s.averageRating || 0,
            totalReviews: total,
            ratingDistribution: distribution,
            verifiedPurchaseCount: s.verifiedCount || 0
          } as ReviewStats
        };
      })
    );
  }

  getReview(reviewId: number): Observable<ApiResultData<Review>> {
    return this.http.get<ApiResultData<any>>(`${this.apiUrl}/${reviewId}`).pipe(
      map(res => ({ ...res, item: this.mapReview(res.item) }))
    );
  }

  createReview(dto: CreateReview): Observable<ApiResult> {
    return this.http.post<ApiResult>(`${this.apiUrl}`, dto);
  }

  createReviewMultipart(formData: FormData): Observable<ApiResult> {
    return this.http.post<ApiResult>(`${this.apiUrl}`, formData);
  }

  updateReview(reviewId: number, dto: UpdateReview): Observable<ApiResult> {
    return this.http.put<ApiResult>(`${this.apiUrl}/${reviewId}`, dto);
  }

  updateReviewMultipart(reviewId: number, formData: FormData): Observable<ApiResult> {
    return this.http.put<ApiResult>(`${this.apiUrl}/${reviewId}`, formData);
  }

  deleteReview(reviewId: number): Observable<ApiResult> {
    return this.http.delete<ApiResult>(`${this.apiUrl}/${reviewId}`);
  }

  toggleHelpful(reviewId: number): Observable<ApiResult> {
    return this.http.post<ApiResult>(`${this.apiUrl}/${reviewId}/helpful`, {});
  }

  getMyReviews(page: number = 1, pageSize: number = 10): Observable<ApiResultPagination<Review>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ApiResultPagination<any>>(`${this.apiUrl}/my`, { params }).pipe(
      map(res => ({
        ...res,
        items: (res.items || []).map((r: any) => this.mapReview(r))
      }))
    );
  }

  /**
   * Backend returns ApiResult<bool> (item is a raw boolean).
   * We transform it to match CanReviewResult { canReview, reason }.
   */
  canReview(productId: number): Observable<ApiResultData<CanReviewResult>> {
    const params = new HttpParams().set('productId', productId.toString());
    return this.http.get<ApiResultData<boolean>>(`${this.apiUrl}/can-review`, { params }).pipe(
      map(res => ({
        ...res,
        item: { canReview: res.item, reason: res.message }
      }))
    );
  }
}