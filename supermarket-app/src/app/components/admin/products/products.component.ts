import { Component, OnInit, inject, signal, computed, resource} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../../services/product.service';
import { ProductModalComponent } from '../product-modal/product-modal.component';
import { toSignal, } from '@angular/core/rxjs-interop';
import { map,firstValueFrom} from 'rxjs';
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


  /*
  private productsResult$ = combineLatest([
    this.route.queryParams,
    toObservable(this.refreshTrigger)
    ]).pipe(
        switchMap(([params, triggerValue])=>{

        const page = params['page'] ? Number(params['page']) : 1;
        return this.productService.getProducts(undefined, page, this.pageSize())
      })
    )
 */

  productResource = resource({
    // 只要這裡定義的變數（Signal）改變，就會自動觸發下面的 loader
    request: () => ({
      page: this.currentPage(),
      size: this.pageSize()
    }),
    // 執行異步請求（底層必須回傳 Promise，所以用 firstValueFrom 轉換，或改用 http.get 的 Promise 版本）
    loader: async ({ request }) => {
     
      const result = await firstValueFrom(
        this.productService.getProducts(undefined, request.page, request.size)
      );
      return result || { items: [], totalPages: 0 };
    }
  });




  products = computed(() => this.productResource.value()?.items || []);
  totalPages = computed(() => this.productResource.value()?.totalPages|| 0);
  
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
    alert(`切換 ${product.name} 上下架狀態 (功能開發中)`);
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
}
