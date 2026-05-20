import { Component, OnInit,OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { Product, ProductDto } from '../../models/product';
import { Subscription,lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css'
})
export class ProductDetailComponent implements OnInit,OnDestroy{
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private cartService = inject(CartService);
  private routeSub!: Subscription;

  product: Product | null = null;
  isLoading = true;
  quantity: number = 1;
  relatedProducts: ProductDto[] = [];
  

  async ngOnInit(): Promise<void> {
    this.routeSub = this.route.paramMap.subscribe(async (params) => {
      const productId = Number(params.get('id'));
      
      if (productId) {
        this.quantity = 1; 

        await this.loadProductAllData(productId);
      }
    });
  }

  async loadProductAllData(id: number) {
    this.isLoading = true;
    try {
      this.product = await lastValueFrom(this.productService.getProductById(id)) 
      if (this.product) {
        const products = await lastValueFrom(this.productService.getProducts(this.product.categoryId));
        this.relatedProducts = products
          .filter(p => p.id !== id)
          .slice(0, 4);
      }
    } catch (err) {
      console.error('載入商品詳情失敗', err);
    } finally {
      this.isLoading = false;
    }
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
  }

  async addToCart() {
    if (!this.product) return;
    try {
      await this.cartService.addToCart(this.product.id, this.quantity);

      alert(`已成功加入購物車！ 🛒`);
      
    } catch (error) {
      console.error(error);
      alert('加入購物車失敗，請稍後再試');
    }
  }

  changeQuantity(amount: number) {
    this.quantity = Math.max(1, this.quantity + amount);
  }

}


 