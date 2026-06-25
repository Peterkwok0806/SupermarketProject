import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Product, ProductCategory, ProductDto,PagedResult } from '../models/product';
import { environment } from '../../environments/environment';



@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}api/product`;



  getProducts(categoryId?: number, keyword?: string, sortBy?: string, page: number = 1, pageSize: number = 10): Observable<PagedResult<ProductDto>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

       if (categoryId){
          params = params.set('category', categoryId.toString());
       }

       if (keyword && keyword.trim()){
          params = params.set('keyword', keyword.trim());
       }

       if (sortBy && sortBy.trim()){
          params = params.set('sortBy', sortBy.trim());
       }

       return this.http.get<PagedResult<ProductDto>>(this.apiUrl, { params });
  }

  getCategories():Observable<ProductCategory[]>{
    return this.http.get<ProductCategory[]>(`${this.apiUrl}/categories`);
  }

  getProductById(id: number): Observable<Product> {
  return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  searchProducts(keyword: string, page: number = 1, pageSize: number = 10): Observable<PagedResult<ProductDto>> {
    let params = new HttpParams()
    .set('keyword', keyword)
    .set('page', page.toString())
    .set('pageSize', pageSize.toString());
    return this.http.get<PagedResult<ProductDto>>(`${this.apiUrl}/search`, { params });
  }

  getSearchSuggestions(term: string): Observable<string[]> {
    if (!term || !term.trim()) {
      return of([]);
    }

    const params = new HttpParams().set('q', term.trim());
    return this.http.get<string[]>(`${this.apiUrl}/suggestions?`, { params });
  }

  createProduct(formData: FormData):Observable<any>{
      return this.http.post<any>(this.apiUrl, formData);
  }

  updateProduct(id: number, formData: FormData):Observable<any>{
    return this.http.put<any>(`${this.apiUrl}/${id}`, formData)
  }

  toggleAvailability(id: number): Observable<{ success: boolean; message: string }> {
    return this.http.patch<{ success: boolean; message: string }>(
      `${this.apiUrl}/${id}/availability`,
      {}
    );
  }

}