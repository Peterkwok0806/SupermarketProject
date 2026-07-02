import { Component, inject, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { Router } from '@angular/router';
import { BackendImagePipe } from '../../pipes/backend-image.pipe';
import { CouponApiService } from '../../services/coupon-api.service';
import { NotificationService } from '../../services/notification.service';
import { LoggerService } from '../../services/logger.service';
import { CouponValidationResultDto } from '../../models/coupon';

@Component({
  selector: 'app-checkout',
  imports: [CommonModule, FormsModule, BackendImagePipe],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent implements OnInit {
  private cartService = inject(CartService);
  private orderService = inject(OrderService);
  private router = inject(Router);
  private couponApi = inject(CouponApiService);
  private notificationService = inject(NotificationService);
  private logger = inject(LoggerService);

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

  // ===== Coupon State =====
  couponCode = '';
  couponValidating = false;
  couponError = '';
  appliedCoupon: CouponValidationResultDto | null = null;

  finalTotal(): number {
    const subtotal = this.totalPrice();
    if (this.appliedCoupon && this.appliedCoupon.isValid) {
      return Math.max(0, subtotal - this.appliedCoupon.discountAmount);
    }
    return subtotal;
  }

  ngOnInit() {
    if (this.cartItems().length === 0) {
      this.router.navigate(['/cart']);
    }
  }

  // ===== Coupon Methods =====
  onApplyCoupon(): void {
    const code = this.couponCode.trim();
    if (!code) {
      this.couponError = 'Please enter a coupon code';
      return;
    }

    this.couponValidating = true;
    this.couponError = '';
    this.appliedCoupon = null;

    const items = this.cartItems();
    const cartProductIds = items.map(item => Number(item.product.id)).filter(id => !isNaN(id));
    const cartCategoryIds = items
      .map(item => item.product.categoryId)
      .filter((id): id is number => id != null && !isNaN(Number(id)))
      .map(id => Number(id));

    this.couponApi.validateCoupon({
      code: code,
      orderSubtotal: this.totalPrice(),
      cartProductIds: cartProductIds,
      cartCategoryIds: cartCategoryIds
    }).subscribe({
      next: (res) => {
        this.couponValidating = false;
        if (res.success && res.item) {
          if (res.item.isValid) {
            this.appliedCoupon = res.item;
            this.couponError = '';
          } else {
            this.couponError = res.item.errorMessage || 'Invalid coupon';
          }
        } else {
          this.couponError = res.message || 'Failed to validate coupon';
        }
      },
      error: (err) => {
        this.couponValidating = false;
        this.couponError = 'Failed to validate coupon. Please try again.';
        this.logger.error('Coupon validation error:', err);
      }
    });
  }

  onRemoveCoupon(): void {
    this.appliedCoupon = null;
    this.couponCode = '';
    this.couponError = '';
  }

  // ===== Submit Order =====
  async onSubmitOrder() {
    if (!this.orderData.fullName || !this.orderData.phone || !this.orderData.address) {
      this.notificationService.error("請填寫完整收貨資料");
      return;
    }

    if (this.cartItems().length === 0) {
      this.notificationService.error("購物車是空的");
      return;
    }

    try {
      // Pass coupon code to order service so it can be applied after order creation
      const couponCode = this.appliedCoupon?.code || undefined;
      await this.orderService.SubmitOrder(this.orderData, couponCode);
    } catch (err) {
      this.logger.error('提交訂單失敗', err);
    }
  }
}
