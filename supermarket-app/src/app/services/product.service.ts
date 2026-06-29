import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { Product, ProductCategory, ProductDto, PagedResult } from '../models/product';
import { LowStockAlert } from '../models/dashboard';
import { ApiResult, ApiResultData } from '../models/api-result';
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

    if (categoryId) {
      params = params.set('category', categoryId.toString());
    }

    if (keyword && keyword.trim()) {
      params = params.set('keyword', keyword.trim());
    }

    if (sortBy && sortBy.trim()) {
      params = params.set('sortBy', sortBy.trim());
    }

    return this.http.get<PagedResult<ProductDto>>(this.apiUrl, { params });
  }

  getCategories(): Observable<ProductCategory[]> {
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
    return this.http.get<string[]>(`${this.apiUrl}/suggestions`, { params });
  }

  createProduct(formData: FormData): Observable<any> {
    return this.http.post<any>(this.apiUrl, formData);
  }

  updateProduct(id: number, formData: FormData): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, formData);
  }

  toggleAvailability(id: number): Observable<{ success: boolean; message: string }> {
    return this.http.patch<{ success: boolean; message: string }>(
      `${this.apiUrl}/${id}/availability`,
      {}
    );
  }

  getLowStockAlert(threshold: number = 10): Observable<LowStockAlert> {
    const params = new HttpParams().set('threshold', threshold.toString());
    return this.http.get<ApiResultData<LowStockAlert>>(`${this.apiUrl}/low-stock-alert`, { params }).pipe(
      map(res => res.item)
    );
  }

  batchToggleAvailability(productIds: number[], isAvailable: boolean): Observable<ApiResult> {
    return this.http.post<ApiResult>(`${this.apiUrl}/batch/toggle-availability`, { productIds, isAvailable });
  }

  batchSoftDelete(productIds: number[]): Observable<ApiResult> {
    return this.http.post<ApiResult>(`${this.apiUrl}/batch/soft-delete`, { productIds });
  }

  /**
   * 匯出所有商品為 Excel (.xlsx) 檔案。
   * 後端回傳 Blob，由呼叫端負責觸發下載。
   * Auth 會由 authInterceptor 自動加上 Bearer token。
   */
  exportProducts(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export`, {
      responseType: 'blob'
    });
  }

  /**
   * 從使用者選取的 .xlsx 檔案批次匯入商品。
   * 後端會根據「商品分類名稱」自動尋找或新建對應的 ProductCategory。
   * @param file 使用者選取的 Excel 檔案
   */
  importProducts(file: File): Observable<ApiResult> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResult>(`${this.apiUrl}/import`, formData);
  }
}
