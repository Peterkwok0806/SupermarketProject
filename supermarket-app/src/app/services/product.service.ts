import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product, ProductCategory } from '../models/product';


@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7154/api/Product';

  

  getProducts(categoryId?: number): Observable<Product[]> {
    let url = this.apiUrl;
    if (categoryId !== undefined) {
      url += `?category=${categoryId}`;
    }
    return this.http.get<Product[]>(url);
  }

  getCategories():Observable<ProductCategory[]>{
    return this.http.get<ProductCategory[]>(`${this.apiUrl}/categories`);
  }

}
