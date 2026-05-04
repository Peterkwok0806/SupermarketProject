import { Component, OnInit, inject } from '@angular/core';
import { Product } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { CategoryNamePipe } from '../../pipes/category-name.pipe';



@Component({
  selector: 'app-productlist',
  imports: [CommonModule, CategoryNamePipe],
  templateUrl: './productlist.component.html',
  styleUrl: './productlist.component.css'
})
export class ProductlistComponent implements OnInit{

  private productService = inject(ProductService);
  private cartService = inject(CartService);

  products$!: Observable<Product[]>;

  ngOnInit(): void {
      this.products$ = this.productService.getProducts();
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
