import { Component, OnInit, inject } from '@angular/core';
import { Product, ProductCategory, ProductDto } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { lastValueFrom } from 'rxjs';
import { RouterLink, ActivatedRoute  } from '@angular/router'; 
import { switchMap } from 'rxjs/operators';




@Component({
  selector: 'app-productlist',
  imports: [CommonModule, RouterLink],
  templateUrl: './productlist.component.html',
  styleUrl: './productlist.component.css'
})
export class ProductlistComponent implements OnInit{

  private productService = inject(ProductService);
  private cartService = inject(CartService);
  private route = inject(ActivatedRoute);

  products$!: Observable<ProductDto[]>;
  categories: ProductCategory[] = [];

  selectedCategory: number | null = null;
  searchTerm :string = '';

  async ngOnInit(): Promise<void> {
      await this.loadCategories();

      this.products$ = this.route.queryParams.pipe(
      switchMap(params => {
        this.searchTerm = params['search'] || ''; // 同步搜尋字串到畫面
        
        if (this.searchTerm.trim()) {
          this.selectedCategory = null; // 有搜尋字，取消分類篩選
          return this.productService.searchProducts(this.searchTerm.trim());
        } else {
          // 沒有搜尋字，走原本的分類邏輯 (此處傳入目前的選中分類或預設 null)
          const searchId = this.selectedCategory ?? undefined;
          return this.productService.getProducts(searchId);
        }
      })
    );
    }

  async loadCategories() {
    try{
      this.categories = await lastValueFrom(this.productService.getCategories());
    }catch (err){
      console.error('載入分類失敗', err);
    }
  }

  filterByCategory(categoryId: number | null) {
    this.selectedCategory = categoryId;
    this.searchTerm = '';
    const searchId = categoryId ?? undefined;
    this.products$ = this.productService.getProducts(searchId);
  }

  onSearch() {
    if (this.searchTerm.trim()) {
      this.selectedCategory = null; // 取消分類篩選
      this.products$ = this.productService.searchProducts(this.searchTerm.trim());
    } else {
      this.filterByCategory(null);
    }
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

  
}
