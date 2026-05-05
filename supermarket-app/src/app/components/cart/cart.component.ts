import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'app-cart',
  imports: [CommonModule, RouterLink],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class CartComponent {
  public cartService = inject(CartService);

  cart = this.cartService.cart;
  totalItems = this.cartService.totalItems;
  totalPrice = this.cartService.totalPrice;
  isLoading = this.cartService.isLoading;

  async updateQuantity(productId: number, quantity: number) {
    await this.cartService.updateQuantity(productId, quantity);
  }

  async removeItem(productId: number) {
    await this.cartService.removeFromCart(productId);
  }

  async clearCart() {
    if (confirm('Clear all items in cart?')) {
      await this.cartService.clearCart();
    }
  }

}
