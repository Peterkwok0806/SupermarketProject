import { Component, OnInit, inject, signal, computed, } from '@angular/core';
import { Product, ProductCategory, ProductDto } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { SearchService } from '../../services/search.service';
import { CartService } from '../../services/cart.service';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { lastValueFrom } from 'rxjs';
import { RouterLink, ActivatedRoute, Router} from '@angular/router'; 
import { switchMap, map } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';





@Component({
  selector: 'app-productlist',
  imports: [CommonModule, RouterLink],
  templateUrl: './productlist.component.html',
  styleUrl: './productlist.component.css'
})
export class ProductlistComponent implements OnInit{

  private productService = inject(ProductService);
  private cartService = inject(CartService);
  private searchService = inject(SearchService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  categories: ProductCategory[] = [];

  pageSize = signal<number>(10);
  selectedCategory = signal<number | null>(null);

  searchTerm = toSignal(
    this.route.queryParams.pipe(
      map(params => {
        const search = params['search'] || '';
        this.searchService.searchInputValue.set(search); // 💡 關鍵：讓 Header 輸入框與網址字串保持絕對同步！
        return search;
      })
    ),
    { initialValue: '' }
  );

  currentPage = toSignal(
    this.route.queryParams.pipe(
      map(params => {
        const page = params['page'] ? Number(params['page']) : 1;
        return page < 1 ? 1 : page; // 防禦性防錯，確保頁碼不小於 1
      })
    ),
    { initialValue: 1 }
  );

  private productsResult$ = this.route.queryParams.pipe(
    switchMap(params => {
      const search = params['search'] || '';
      const page = params['page'] ? Number(params['page']) : 1;
      const catId = params['catId'] ? Number(params['catId']) : 1;
      
      if (search.trim()) {
        this.selectedCategory.set(null); // 清空分類
        return this.productService.searchProducts(search.trim(), page, this.pageSize());
      } else {
        return this.productService.getProducts(catId, page, this.pageSize());
      }
    })
  );

  pagedResult = toSignal(this.productsResult$);

  products = computed(() => this.pagedResult()?.items || []);
  totalPages = computed(() => this.pagedResult()?.totalPages || 0);
  totalCount = computed(() => this.pagedResult()?.totalCount || 0);

  navigatePage(pageNumber: number): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page: pageNumber },
      queryParamsHandling: 'merge' // 保持現有的 search 或 category 參數不變
    });
  }

  async ngOnInit(): Promise<void> {
      await this.loadCategories();
  }

  async loadCategories() {
    try{
      this.categories = await lastValueFrom(this.productService.getCategories());
    }catch (err){
      console.error('載入分類失敗', err);
    }
  }

  


  filterByCategory(categoryId: number | null) {
  // 1. 更新本地的分類 Signal 狀態，驅動畫面按鈕的高亮樣式
  this.selectedCategory.set(categoryId);

  // 2. 一次性更新 URL 參數，重設頁碼為 1，並直接在網址清空搜尋字串
  this.router.navigate([], {
    relativeTo: this.route,
    queryParams: {
      catId :this.selectedCategory(),
      page: 1, 
      search: null // 點擊分類時，主動把網址上的 ?search=xxx 拔掉，維持邏輯乾淨
    }, 
    queryParamsHandling: 'merge'
  });
}
  


  async addToCart(prodcutid: number, quantity: number ) {
    try {
      await this.cartService.addToCart(prodcutid, quantity);

      alert(`已成功加入購物車！ 🛒`);
      
    } catch (error) {
      console.error(error);
      alert('加入購物車失敗，請稍後再試');
    }
  }

  // 點擊「下一頁」按鈕
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

  
}
