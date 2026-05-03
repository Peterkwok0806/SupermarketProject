import { Injectable, signal, computed, effect } from '@angular/core';
import { CartItem } from '../models/cart-item';
import { Product } from '../models/product';


@Injectable({
  providedIn: 'root'
})
export class CartService {
  private _cartItems = signal<CartItem[]>([]);
  readonly cartItems = this._cartItems.asReadonly();

  private saveToLocalStorage(items: CartItem[]) {
    localStorage.setItem('cart', JSON.stringify(items));
  }

  private loadFromLocalStorage() {
    const saved = localStorage.getItem('cart');
    if (saved) {
      try {
        this._cartItems.set(JSON.parse(saved));
      } catch (e) {
        console.error('Failed to load cart from localStorage', e);
      }
    }
  }

  totalItems = computed(() => 
    this._cartItems().reduce((sum, item) => sum + item.quantity, 0)
  );

  totalPrice = computed(() => 
    this._cartItems().reduce((sum, item) => 
      sum + (item.product.price * item.quantity), 0)
  );


  constructor() { 
    this.loadFromLocalStorage();

    effect(() => {
      this.saveToLocalStorage(this._cartItems());
    });
  }

  addToCart(product: Product) {
    this._cartItems.update(current => {
      const index = current.findIndex(item => item.product.id === product.id);
      const updated = [...current];
      if (index > -1) {
        updated[index] = { ...updated[index], quantity: updated[index].quantity + 1 };
      } else {
        updated.push({ product, quantity: 1 });
      }
      return updated;
    });
    
  }

  removeFromCart(productId: number) {
    this._cartItems.update(current => current.filter(item => item.product.id !== productId));
  }

  updateQuantity(productId: number, quantity: number) {
    if (quantity <= 0) {
      this.removeFromCart(productId);
      return;
    }

    this._cartItems.update(current => 
      current.map(item => item.product.id === productId ? { ...item, quantity } : item)
    );
  }


  clearCart() {
      this._cartItems.set([]);
  }

}
