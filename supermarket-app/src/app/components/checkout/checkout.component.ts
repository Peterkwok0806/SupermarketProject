import { Component, inject, computed,  OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../services/cart.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-checkout',
  imports: [CommonModule, FormsModule],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent {
  private cartService = inject(CartService);
  private router = inject(Router);

  cart = this.cartService.cart;
  totalPrice = this.cartService.totalPrice;
  cartItems = computed(() => this.cart().cartItems);

  orderData = {
    fullName: '',
    phone: '',
    address: '',
    remark: ''
  };

  ngOnInit() {
    if (this.cartItems().length === 0) {
      this.router.navigate(['/cart']);
    }
  }

  async onSubmitOrder() {
    if (!this.orderData.fullName || !this.orderData.phone || !this.orderData.address) {
      alert("請填寫完整收貨資料");
      return;
    }

    if (this.cartItems().length === 0) {
      alert("購物車是空的");
      return;
    }

    // TODO: 之後串接後端建立訂單 API
    alert("🎉 訂單已成功建立！\n\n感謝您的購買！");
    this.cartService.clearCart();
    this.router.navigate(['/']);
  }
  
}
