import { Component, OnInit, inject } from '@angular/core';
import { Product, ProductCategory } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';




@Component({
  selector: 'app-productlist',
  imports: [CommonModule],
  templateUrl: './productlist.component.html',
  styleUrl: './productlist.component.css'
})
export class ProductlistComponent implements OnInit{

  private productService = inject(ProductService);
  private cartService = inject(CartService);

  products$!: Observable<Product[]>;
  categories: ProductCategory[] = [];

  selectedCategory: number | null = null;

  ngOnInit(): void {
      this.loadCategories();
       this.filterByCategory(null);
    }

  loadCategories() {
    this.productService.getCategories().subscribe({
      next: (data) => this.categories = data,
      error: (err) => console.error('載入分類失敗', err)
    });
  }

  filterByCategory(categoryId: number | null) {
    this.selectedCategory = categoryId;
    
    const searchId = categoryId ?? undefined;
    this.products$ = this.productService.getProducts(searchId);
  }


  async addToCart(product: Product) {
    try {
      await this.cartService.addToCart(product);

      alert(`${product.name} 已成功加入購物車！ 🛒`);
      
    } catch (error) {
      console.error(error);
      alert('加入購物車失敗，請稍後再試');
    }
  }

  
}
