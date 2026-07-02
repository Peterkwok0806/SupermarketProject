import { Injectable, inject, signal, computed, effect } from '@angular/core';
import { CartApiService } from './cart-api.service';
import { Cart} from '../models/cart';
import { Product } from '../models/product';
import { firstValueFrom } from 'rxjs';
import { AuthService } from './auth.service';
import { LoggerService } from './logger.service';


@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartApi = inject(CartApiService);
  private authService = inject(AuthService);
  private logger = inject(LoggerService);

  private _cart = signal<Cart>({
    id: 0,
    userId: 0,
    cartItems: [],
    totalAmount:0
  });
  readonly cart = this._cart.asReadonly();

  private initialCart: Cart = {
  id: 0,
  userId: 0,
  cartItems: [],
  totalAmount:0
  };

  isLoading = signal<boolean>(false);
  totalPrice = computed(()=>this._cart().totalAmount)

  // 計算屬性
  totalItems = computed(() => {
    const items = this._cart().cartItems;
    return items.reduce((sum, item) => sum + item.quantity, 0);
  });


  constructor() { 
      effect(() => {
      if (this.authService.isLoggedIn()) {
        this.logger.log('偵測到已登入，開始載入購物車');
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
    } catch (err) {
      this.logger.error('無法取得購物車', err);
    }
  }

  async addToCart(productId: number,  quantity: number) {
    this.isLoading.set(true);
    try {
        const result = await firstValueFrom(this.cartApi.addToCart(productId,quantity));

        if (result.success && result.cart){
          this._cart.set({ ...result.cart });
          this.logger.log('購物車已更新');
        }
      } catch (err) {
        this.logger.error('Add failed', err);
        throw err;
      }finally {
        this.isLoading.set(false); 
      }
    }

  async updateQuantity(productId: number, quantity: number) {
  if (quantity < 1) return;

  this.isLoading.set(true);
  try {
    const result = await firstValueFrom(this.cartApi.updateQuantity(productId, quantity));
    if (result.success && result.cart){
          this._cart.set({ ...result.cart });
        }
  } catch (error) {
    this.logger.error('更新數量失敗', error);
  }finally {
      this.isLoading.set(false);
    }
}

 async removeFromCart(productId: number) {
   this.isLoading.set(true);
  try {
    const result = await firstValueFrom(this.cartApi.removeFromCart(productId));
    if (result.success && result.cart){
          this._cart.set({ ...result.cart });
        }
  } catch (error) {
    this.logger.error('移除商品失敗', error);
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
      }
    }catch (error) {
      this.logger.error('Clear cart failed', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  resetCart() {
    this._cart.set(this.initialCart);
  }

}
