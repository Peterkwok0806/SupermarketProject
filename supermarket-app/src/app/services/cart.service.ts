import { Injectable, inject, signal, computed } from '@angular/core';
import { CartApiService } from './cart-api.service';
import { Cart} from '../models/cart';
import { Product } from '../models/product';
import { firstValueFrom } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartApi = inject(CartApiService);

  private _cart = signal<Cart>({
    id: 0,
    userId: 0,
    cartItems: []
  });
  readonly cart = this._cart.asReadonly();

  isLoading = signal<boolean>(false);


  // 計算屬性
  totalItems = computed(() => {
    const items = this._cart().cartItems;
    return items.reduce((sum, item) => sum + item.quantity, 0);
  });

  totalPrice = computed(() => {
    const items = this._cart().cartItems;
    // 優先使用 unitPrice (Dto 內通常已包含當時價格)，若無則用 product.price
    return items.reduce((sum, item) => 
      sum + ((item.unitPrice || item.product.price) * item.quantity), 0);
  });

  constructor() { 
    this.loadCart();
  }

  async loadCart() {
    try {
      const data = await firstValueFrom(this.cartApi.getCart());
      this._cart.set(data);
      
    } catch (err) {
      // 處理 API 錯誤（例如：404 或網路斷線）
      console.error('無法取得購物車', err);
    }
  }

 async addToCart(product: Product) {
  console.log('1. 開始執行 addToCart, ID:', product.id);
  this.isLoading.set(true);
    try {
        console.log('2. 準備發送 API...');
        const result = await firstValueFrom(this.cartApi.addToCart(product.id));
        console.log('3. API 回傳結果:', result);

        console.log(Boolean(result.success && result.cart))

        if (result.success && result.cart){
          this._cart.set({ ...result.cart });
          console.log('4. Signal 已更新');
        }
      } catch (err) {
        console.error('Add failed', err);
      }finally {
        this.isLoading.set(false); // 結束讀取
      }
    }

  async updateQuantity(productId: number, quantity: number) {
  if (quantity < 1) return;

  this.isLoading.set(true);
  try {
    const result = await firstValueFrom(this.cartApi.updateQuantity(productId, quantity));
    console.log('3. API 回傳結果:', result);
    if (result.success && result.cart) {
      this._cart.set({ ...result.cart });
    }
  } catch (error) {
    console.error('更新數量失敗', error);
  }finally {
      this.isLoading.set(false);
    }
}

 async removeFromCart(productId: number) {
   this.isLoading.set(true);
  try {
    const result = await firstValueFrom(this.cartApi.removeFromCart(productId));
    if (result.success && result.cart) {
      this._cart.set({ ...result.cart });
    }
  } catch (error) {
    console.error('移除商品失敗', error);
  }finally {
      this.isLoading.set(false);
  }
}

  async clearCart() {
   this.isLoading.set(true);
    try {
      const result= await firstValueFrom(this.cartApi.clearCart());
      if (result.success && result.cart) {
      this._cart.set({ ...result.cart });
    }
      console.log('購物車已清空並刷新');
    } catch (error) {
      console.error('Clear cart failed', error);
    } finally {
      this.isLoading.set(false);
    }
  }

}
