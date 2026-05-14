import { Component, inject, computed,  OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-checkout',
  imports: [CommonModule, FormsModule],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent {
  private cartService = inject(CartService);
  private orderService = inject(OrderService);
  private router = inject(Router);

  cart = this.cartService.cart;
  totalPrice = this.cartService.totalPrice;
  cartItems = computed(() => this.cart().cartItems);
  isSubmitting = this.orderService.isSubmitting;

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

    try{
      await this.orderService.SubmitOrder(this.orderData)
    }catch (err) {
    console.error(err);
    }
  }

   
  
}
