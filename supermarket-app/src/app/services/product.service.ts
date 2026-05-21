import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Product, ProductCategory, ProductDto } from '../models/product';



@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7154/api/Product';

  

  getProducts(categoryId?: number): Observable<ProductDto[]> {
    let url = this.apiUrl;
    if (categoryId !== undefined) {
      url += `?category=${categoryId}`;
    }
    return this.http.get<ProductDto[]>(url);
  }

  getCategories():Observable<ProductCategory[]>{
    return this.http.get<ProductCategory[]>(`${this.apiUrl}/categories`);
  }

  getProductById(id: number): Observable<Product> {
  return this.http.get<Product>(`${this.apiUrl}/${id}`);
}

  searchProducts(keyword: string): Observable<ProductDto[]> {
    return this.http.get<ProductDto[]>(`${this.apiUrl}/search?keyword=${encodeURIComponent(keyword)}`);
  }

  getSearchSuggestions(term: string): Observable<string[]> {
    if (!term || !term.trim()) {
    return of([]);
  }
    return this.http.get<string[]>(`${this.apiUrl}/suggestions?q=${term}`);
  }

}
