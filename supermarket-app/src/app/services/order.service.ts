import { Injectable, inject, signal } from '@angular/core';
import { OrderApiService } from './order-api.service';
import { OrderRequest, OrderEntity } from '../models/order';
import { lastValueFrom } from 'rxjs';
import { CartService } from './cart.service';
import { Router } from '@angular/router';
import { CouponApiService } from './coupon-api.service';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  private orderApi = inject(OrderApiService);
  private router = inject(Router);
  private cartService = inject(CartService);
  private couponApi = inject(CouponApiService);

 isSubmitting = signal<Boolean>(false);
 currentOrder = signal<OrderEntity | null>(null);
 isProcessing = signal<boolean>(false);
 orders = signal<OrderEntity[]>([]);

  async SubmitOrder(data: OrderRequest, couponCode?: string) {
    this.isSubmitting.set(true);
    try {
      const orderPayload: OrderRequest = {
      ...data,
      couponCode: couponCode ?? null
    };

      const response = await lastValueFrom(this.orderApi.createOrder(orderPayload));
      if (response.success && response?.order?.snowflakeId) {
        this.cartService.clearCart();
        await this.router.navigate(['/order-success'], {
          queryParams: { snowflakeId: response.order.snowflakeId }
        });
      }
    } catch (error: any) {
      console.error(error);
    } finally {
      this.isSubmitting.set(false);
    }
  }

  async loadOrderDetail(orderSnowflakeId: string){
    this.isProcessing.set(false);
    try{
      const response = await lastValueFrom(this.orderApi.getOrderById(orderSnowflakeId));
      this.currentOrder.set(response);
    }catch(error){
      console.error('邏輯層：獲取訂單詳細失敗', error);
      this.currentOrder.set(null);
    }finally{
        this.isProcessing.set(false);
    }
  }

  async loadOrders(){
    try{
      const response = await lastValueFrom(this.orderApi.getMyOrders());
      this.orders.set(response);
    }catch(error){
       console.error('獲取訂單失敗', error);
    }

  }

  constructor() { }
}
