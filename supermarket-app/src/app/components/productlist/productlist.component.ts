import { Component, OnInit, inject } from '@angular/core';
import { Product, ProductCategory, ProductDto } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { lastValueFrom } from 'rxjs';
import { RouterLink } from '@angular/router'; 




@Component({
  selector: 'app-productlist',
  imports: [CommonModule, RouterLink],
  templateUrl: './productlist.component.html',
  styleUrl: './productlist.component.css'
})
export class ProductlistComponent implements OnInit{

  private productService = inject(ProductService);
  private cartService = inject(CartService);

  products$!: Observable<ProductDto[]>;
  categories: ProductCategory[] = [];

  selectedCategory: number | null = null;

  async ngOnInit(): Promise<void> {
      await this.loadCategories();
       this.filterByCategory(null);
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
    
    const searchId = categoryId ?? undefined;
    this.products$ = this.productService.getProducts(searchId);
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
