import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Cart, CartOperationResult } from '../models/cart';

@Injectable({
  providedIn: 'root'
})
export class CartApiService {

  private apiUrl = 'https://localhost:7154/api/cart';

  private http = inject(HttpClient);

  private getHeaders() {
    const token = localStorage.getItem('token');
    return {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      })
    };
  }

  getCart(): Observable<Cart> {
    return this.http.get<Cart>(this.apiUrl, this.getHeaders());
  }

  addToCart(productId: number, quantity: number = 1): Observable<CartOperationResult> {
    return  this.http.post<CartOperationResult>(`${this.apiUrl}/add`, { productId, quantity }, this.getHeaders());
  }

  updateQuantity(productId: number, quantity: number): Observable<CartOperationResult>{
    return this.http.post<CartOperationResult>(`${this.apiUrl}/update`, { productId, quantity }, this.getHeaders());
  }

  removeFromCart(productId: number): Observable<CartOperationResult> {
    return this.http.delete<CartOperationResult>(`${this.apiUrl}/remove/${productId}`, this.getHeaders());
  }

  clearCart(): Observable<CartOperationResult> {
    return this.http.delete<CartOperationResult>(`${this.apiUrl}/clear`, this.getHeaders());
  }

  constructor() { }
}
