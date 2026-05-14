import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Cart, CartOperationResult, CartSummary } from '../models/cart';

@Injectable({
  providedIn: 'root'
})
export class CartApiService {

  private apiUrl = 'https://localhost:7154/api/cart';

  private http = inject(HttpClient);

  

  getCart(): Observable<CartOperationResult> {
    return this.http.get<CartOperationResult>(this.apiUrl);
  }

  addToCart(productId: number, quantity: number = 1): Observable<CartOperationResult> {
    return  this.http.post<CartOperationResult>(`${this.apiUrl}/add`, { productId, quantity });
  }

  updateQuantity(productId: number, quantity: number): Observable<CartOperationResult>{
    return this.http.post<CartOperationResult>(`${this.apiUrl}/update`, { productId, quantity });
  }

  removeFromCart(productId: number): Observable<CartOperationResult> {
    return this.http.delete<CartOperationResult>(`${this.apiUrl}/remove/${productId}`);
  }

  clearCart(): Observable<CartOperationResult> {
    return this.http.delete<CartOperationResult>(`${this.apiUrl}/clear`);
  }


  constructor() { }
}
