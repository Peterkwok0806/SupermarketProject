import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { Product } from '../../models/product';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css'
})
export class ProductDetailComponent implements OnInit{
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private cartService = inject(CartService);

  product: Product | null = null;
  isLoading = true;

  async ngOnInit(): Promise<void> {
    // 1. 從網址抓取 id (例如: /products/5)
    const productId = Number(this.route.snapshot.paramMap.get('id'));
    
    if (productId) {
      await this.loadProductDetail(productId);
    }
  }

  async loadProductDetail(id: number) {
    try {
      this.isLoading = true;
      // 如果回傳 Observable，記得像之前一樣用 lastValueFrom(this.productService.getProductById(id))
      this.product = await lastValueFrom(this.productService.getProductById(id)) 
    } catch (err) {
      console.error('載入商品詳情失敗', err);
    } finally {
      this.isLoading = false;
    }
  }

  async addToCart() {
    if (!this.product) return;
    try {
      await this.cartService.addToCart(this.product.id);

      alert(`已成功加入購物車！ 🛒`);
      
    } catch (error) {
      console.error(error);
      alert('加入購物車失敗，請稍後再試');
    }
  }
}


 