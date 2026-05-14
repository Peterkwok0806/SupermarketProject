import { Injectable, inject, signal, computed, effect } from '@angular/core';
import { CartApiService } from './cart-api.service';
import { Cart} from '../models/cart';
import { Product } from '../models/product';
import { firstValueFrom } from 'rxjs';
import { AuthService } from './auth.service';


@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartApi = inject(CartApiService);
  private authService = inject(AuthService);

  private _cart = signal<Cart>({
    id: 0,
    userId: 0,
    cartItems: []
  });
  readonly cart = this._cart.asReadonly();

  private initialCart: Cart = {
  id: 0,
  userId: 0,
  cartItems: []
  };

  isLoading = signal<boolean>(false);
  totalPrice = signal<number>(0);

  // 計算屬性
  totalItems = computed(() => {
    const items = this._cart().cartItems;
    return items.reduce((sum, item) => sum + item.quantity, 0);
  });


  constructor() { 
      effect(() => {
      if (this.authService.isLoggedIn()) {
        console.log('偵測到已登入，開始載入購物車');
        this.loadCart();
      } else {
        this.resetCart();
      }
    }, { allowSignalWrites: true });
  }

  async loadCart() {
    try {
      const respones = await firstValueFrom(this.cartApi.getCart());
      this._cart.set(respones.cart);
      this.totalPrice.set(respones.totalAmount);
      
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

        if (result.success && result.cart && result.totalAmount){
          this._cart.set({ ...result.cart });
          this.totalPrice.set(result.totalAmount);
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
    if (result.success && result.cart && result.totalAmount){
          this._cart.set({ ...result.cart });
          this.totalPrice.set(result.totalAmount);
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
    if (result.success && result.cart && result.totalAmount){
          this._cart.set({ ...result.cart });
          this.totalPrice.set(result.totalAmount);
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
      const result = await firstValueFrom(this.cartApi.clearCart());
      if(result.success){
        this._cart.set(this.initialCart);
        this.totalPrice.set(0);
      console.log('購物車已清空並刷新');
      }
    }catch (error) {
      console.error('Clear cart failed', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  resetCart() {
    this._cart.set(this.initialCart);
  }

}
