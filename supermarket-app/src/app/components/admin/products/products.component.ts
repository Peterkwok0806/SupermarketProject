import { Component, inject, signal, computed, resource } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../services/product.service';
import { NotificationService } from '../../../services/notification.service';
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
  private notificationService = inject(NotificationService);
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

  // === 批量操作 ===
  selectedProductIds = signal<number[]>([]);
  isBatchLoading = signal(false);
  hasSelection = computed(() => this.selectedProductIds().length > 0);
  isAllSelected = computed(() => {
    const prods = this.products();
    if (prods.length === 0) return false;
    const selected = this.selectedProductIds();
    return prods.every(p => selected.includes(p.id));
  });

  // === 匯入 / 匯出狀態 ===
  isExporting = signal(false);
  isImporting = signal(false);

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
    // 執行異步請求（底層必須回傳 Promise，所以用 firstValueFrom 轉換）
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
    this.clearSelection();
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

  // === 批量操作方法 ===
  toggleSelectAll(): void {
    if (this.isAllSelected()) {
      // Deselect only current page items
      const currentPageIds = new Set(this.products().map(p => p.id));
      this.selectedProductIds.set(this.selectedProductIds().filter(id => !currentPageIds.has(id)));
    } else {
      // Select all current page items (merge with any existing selections)
      const currentPageIds = this.products().map(p => p.id);
      const merged = new Set([...this.selectedProductIds(), ...currentPageIds]);
      this.selectedProductIds.set([...merged]);
    }
  }

  toggleSelectOne(productId: number): void {
    const current = this.selectedProductIds();
    if (current.includes(productId)) {
      this.selectedProductIds.set(current.filter(id => id !== productId));
    } else {
      this.selectedProductIds.set([...current, productId]);
    }
  }

  isSelected(productId: number): boolean {
    return this.selectedProductIds().includes(productId);
  }

  clearSelection(): void {
    this.selectedProductIds.set([]);
  }

  batchToggleAvailability(isAvailable: boolean): void {
    if (this.isBatchLoading()) return;
    const ids = this.selectedProductIds();
    if (ids.length === 0) return;
    this.isBatchLoading.set(true);
    this.productService.batchToggleAvailability(ids, isAvailable).subscribe({
      next: (res) => {
        this.isBatchLoading.set(false);
        if (res.success) {
          alert(res.message);
          this.clearSelection();
          this.productResource.reload();
        } else {
          alert(res.message);
        }
      },
      error: () => {
        this.isBatchLoading.set(false);
        alert('批量操作失敗');
      }
    });
  }

  batchSoftDelete(): void {
    if (this.isBatchLoading()) return;
    const ids = this.selectedProductIds();
    if (ids.length === 0) return;
    const confirmed = confirm(`確定要刪除 ${ids.length} 項商品？此操作不可復原！`);
    if (!confirmed) return;
    this.isBatchLoading.set(true);
    this.productService.batchSoftDelete(ids).subscribe({
      next: (res) => {
        this.isBatchLoading.set(false);
        if (res.success) {
          alert(res.message);
          this.clearSelection();
          this.productResource.reload();
        } else {
          alert(res.message);
        }
      },
      error: () => {
        this.isBatchLoading.set(false);
        alert('批量刪除失敗');
      }
    });
  }

  // === 匯出 Excel ===
  exportProducts(): void {
    this.isExporting.set(true);
    this.productService.exportProducts().subscribe({
      next: (blob) => {
        // 產生暫時 Blob URL 並觸發下載
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Products_匯出.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        // 釋放記憶體
        setTimeout(() => URL.revokeObjectURL(url), 5000);
        this.isExporting.set(false);
        this.notificationService.success('匯出成功，檔案已下載');
      },
      error: (err) => {
        this.isExporting.set(false);
        this.notificationService.error('匯出失敗：' + (err?.error?.message || '未知錯誤'));
      }
    });
  }

  // === 匯入 Excel ===
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const file = input.files[0];
    input.value = ''; // 清空，避免重複選同一檔觸發不了

    this.isImporting.set(true);
    this.productService.importProducts(file).subscribe({
      next: (res) => {
        this.isImporting.set(false);
        if (res.success) {
          this.notificationService.success(res.message);
          this.productResource.reload(); // 自動刷新列表
        } else {
          this.notificationService.error(res.message);
        }
      },
      error: (err) => {
        this.isImporting.set(false);
        this.notificationService.error('匯入失敗：' + (err?.error?.message || '未知錯誤'));
      }
    });
  }

  // 篩選 / 搜尋 / 排序的處理
  onSearchChange(value: string): void {
    this.searchKeyword.set(value);
    this.clearSelection();
    this.resetToFirstPage();
  }

  onCategoryChange(value: string): void {
    if (value === '' || value === 'all') {
      this.selectedCategoryId.set(null);
    } else {
      this.selectedCategoryId.set(Number(value));
    }
    this.clearSelection();
    this.resetToFirstPage();
  }

  onSortChange(value: string): void {
    this.sortBy.set(value as ProductSortBy);
    this.clearSelection();
    this.resetToFirstPage();
  }

  resetFilters(): void {
    this.searchKeyword.set('');
    this.selectedCategoryId.set(null);
    this.sortBy.set('');
    this.clearSelection();
    this.resetToFirstPage();
  }

  private resetToFirstPage(): void {
    if (this.currentPage() !== 1) {
      this.navigatePage(1);
    }
  }
}
