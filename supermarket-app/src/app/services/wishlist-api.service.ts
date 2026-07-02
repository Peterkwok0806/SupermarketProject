import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WishlistOperationResult } from '../models/wishlist';
import { ProductDto } from '../models/product';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class WishlistApiService {

  private apiUrl = `${environment.apiUrl}api/wishlist`;

  private http = inject(HttpClient);

  getWishlist(): Observable<{ success: boolean; message: string; item: ProductDto[] }> {
    return this.http.get<{ success: boolean; message: string; item: ProductDto[] }>(this.apiUrl);
  }

  addToWishlist(productId: number): Observable<WishlistOperationResult> {
    return this.http.post<WishlistOperationResult>(this.apiUrl, { productId });
  }

  removeFromWishlist(productId: number): Observable<WishlistOperationResult> {
    return this.http.delete<WishlistOperationResult>(`${this.apiUrl}/${productId}`);
  }

  checkInWishlist(productId: number): Observable<{ isInWishlist: boolean }> {
    return this.http.get<{ isInWishlist: boolean }>(`${this.apiUrl}/check/${productId}`);
  }
}
