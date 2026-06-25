import { Component, OnInit, inject, signal, computed, resource } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../services/product.service';
import { ProductModalComponent } from '../product-modal/product-modal.component';
import { BackendImagePipe } from '../../../pipes/backend-image.pipe';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, firstValueFrom } from 'rxjs';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ProductCategory } from '../../../models/product';

export type ProductSortBy = '' | 'name_asc' | 'name_desc' | 'price_asc' | 'price_desc';

@Component({
  selector: 'app-products',
  imports: [CommonModule, FormsModule, ProductModalComponent, BackendImagePipe],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class AdminProductsComponent {
  private productService = inject(ProductService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  pageSize = signal<number>(10);

  showModal = false;
  selectedProductid: any = null;

  // 篩選 / 搜尋 / 排序的 Signal
  searchKeyword = signal<string>('');
  selectedCategoryId = signal<number | null>(null);
  sortBy = signal<ProductSortBy>('');

  // 載入分類清單
  categories = signal<ProductCategory[]>([]);

  // 哪個商品正在切換上下架，避免重複點擊
  togglingId: number | null = null;

  currentPage = toSignal(
    this.route.queryParams.pipe(
      map(params => {
        const page = params['page'] ? Number(params['page']) : 1;
        return page < 1 ? 1 : page;
      })
    ),
    { initialValue: 1 }
  );

  constructor() {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.productService.getCategories().subscribe({
      next: (data) => this.categories.set(data || []),
      error: () => this.categories.set([])
    });
  }

  productResource = resource({
    // 只要這裡定義的變數（Signal）改變，就會自動觸發下面的 loader
    request: () => ({
      page: this.currentPage(),
      size: this.pageSize(),
      keyword: this.searchKeyword(),
      categoryId: this.selectedCategoryId(),
      sortBy: this.sortBy()
    }),
    // 執行異步請求（底層必須回傳 Promise，所以用 firstValueFrom 轉換，或改用 http.get 的 Promise 版本）
    loader: async ({ request }) => {
      const result = await firstValueFrom(
        this.productService.getProducts(
          request.categoryId ?? undefined,
          request.keyword || undefined,
          request.sortBy || undefined,
          request.page,
          request.size
        )
      );
      return result || { items: [], totalPages: 0 };
    }
  });

  products = computed(() => this.productResource.value()?.items || []);
  totalPages = computed(() => this.productResource.value()?.totalPages || 0);

  navigatePage(pageNumber: number): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page: pageNumber },
      queryParamsHandling: 'merge'
    });
  }

  nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.navigatePage(this.currentPage() + 1);
    }
  }

  prevPage(): void {
    if (this.currentPage() > 1) {
      this.navigatePage(this.currentPage() - 1);
    }
  }

  editProduct(prodcutid: number) {
    this.selectedProductid = prodcutid;
    this.showModal = true;
  }

  toggleAvailability(product: any) {
    if (!confirm(`確認要切換「${product.name}」的上下架狀態？`)) {
      return;
    }

    this.togglingId = product.id;
    this.productService.toggleAvailability(product.id).subscribe({
      next: (res) => {
        this.togglingId = null;
        if (res?.success) {
          alert(res.message || '切換成功');
          this.productResource.reload();
        } else {
          alert(res?.message || '切換失敗');
        }
      },
      error: (err) => {
        this.togglingId = null;
        console.error('切換上下架失敗', err);
        alert('切換上下架失敗：' + (err?.error?.message || err?.message || '未知錯誤'));
      }
    });
  }

  openAddProductModal() {
    this.selectedProductid = null;
    this.showModal = true;
  }

  onModalSaved() {
    this.showModal = false;
    this.productResource.reload();
  }

  onModalClosed() {
    this.showModal = false;
  }

  // 篩選 / 搜尋 / 排序的處理
  onSearchChange(value: string): void {
    this.searchKeyword.set(value);
    this.resetToFirstPage();
  }

  onCategoryChange(value: string): void {
    if (value === '' || value === 'all') {
      this.selectedCategoryId.set(null);
    } else {
      this.selectedCategoryId.set(Number(value));
    }
    this.resetToFirstPage();
  }

  onSortChange(value: string): void {
    this.sortBy.set(value as ProductSortBy);
    this.resetToFirstPage();
  }

  resetFilters(): void {
    this.searchKeyword.set('');
    this.selectedCategoryId.set(null);
    this.sortBy.set('');
    this.resetToFirstPage();
  }

  private resetToFirstPage(): void {
    if (this.currentPage() !== 1) {
      this.navigatePage(1);
    }
  }
}