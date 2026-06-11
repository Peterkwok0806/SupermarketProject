import { Component, OnInit, inject, signal, computed, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../../services/product.service';
import { ProductModalComponent } from '../product-modal/product-modal.component';
import { toSignal } from '@angular/core/rxjs-interop';
import { switchMap, map } from 'rxjs';
import { RouterLink, ActivatedRoute, Router} from '@angular/router'; 

@Component({
  selector: 'app-products',
  imports: [CommonModule,ProductModalComponent],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class AdminProductsComponent{
  private productService = inject(ProductService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  pageSize = signal<number>(10);

  showModal = false;
  selectedProductid: any = null;

  currentPage = toSignal(
    this.route.queryParams.pipe(
      map(params => {
        const page = params['page'] ? Number(params['page']) : 1;
        return page < 1 ? 1 : page; 
      })
    ),
    { initialValue: 1 }
  );

  private productsResult$ = this.route.queryParams.pipe(
    switchMap(params => {
      const page = params['page'] ? Number(params['page']) : 1;
        return this.productService.getProducts(undefined,page, this.pageSize());
    })
  );

  pagedResult = toSignal(this.productsResult$);
  products = computed(() => this.pagedResult()?.items || []);
  totalPages = computed(() => this.pagedResult()?.totalPages || 0);
  
  navigatePage(pageNumber: number): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page: pageNumber },
      queryParamsHandling: 'merge' // 保持現有的 search 或 category 參數不變
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
    alert(`切換 ${product.name} 上下架狀態 (功能開發中)`);
  }

  openAddProductModal() {
    this.selectedProductid = null;
    this.showModal = true;
  }

  onModalSaved() {
    this.showModal = false;
  }

  onModalClosed() {
    this.showModal = false;
  }
}
