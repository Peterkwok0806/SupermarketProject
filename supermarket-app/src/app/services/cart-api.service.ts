import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Cart } from '../models/cart';

@Injectable({
  providedIn: 'root'
})
export class CartApiService {

  private apiUrl = 'https://localhost:7154/api/cart';

  private http = inject(HttpClient);

  getCart(): Observable<Cart> {
    return this.http.get<Cart>(this.apiUrl);
  }

  addToCart(productId: number, quantity: number = 1): Observable<any> {
    return this.http.post(`${this.apiUrl}/add`, { productId, quantity });
  }

  updateQuantity(productId: number, quantity: number): Observable<any>{
    return this.http.post(`${this.apiUrl}/update`, { productId, quantity });
  }

  removeFromCart(productId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/remove/${productId}`);
  }

  clearCart(): Observable<any> {
    return this.http.delete(`${this.apiUrl}/clear`);
  }

  constructor() { }
}
