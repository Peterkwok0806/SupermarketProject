 import { Injectable, inject, signal } from '@angular/core';
 import { OrderApiService } from './order-api.service';
 import { OrderRequest, OrderEntity } from '../models/order';
 import { lastValueFrom } from 'rxjs';
 import { CartService } from './cart.service';
 import { Router } from '@angular/router';
 import { CouponApiService } from './coupon-api.service';
 import { ApiResultPagination } from '../models/api-result';
 import { LoggerService } from './logger.service';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  private orderApi = inject(OrderApiService);
  private router = inject(Router);
  private cartService = inject(CartService);
  private couponApi = inject(CouponApiService);
  private logger = inject(LoggerService);

 isSubmitting = signal<boolean>(false);
 currentOrder = signal<OrderEntity | null>(null);
 isProcessing = signal<boolean>(false);
 orders = signal<OrderEntity[]>([]);

 // 分頁狀態
 currentPage = signal<number>(1);
 pageSize = signal<number>(10);
 totalCount = signal<number>(0);
 totalPages = signal<number>(0);

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
      this.logger.error('建立訂單失敗', error);
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
      this.logger.error('邏輯層：獲取訂單詳細失敗', error);
      this.currentOrder.set(null);
    }finally{
        this.isProcessing.set(false);
    }
  }

  async loadOrders(page: number = 1){
    try{
      const response = await lastValueFrom(this.orderApi.getMyOrders(page, this.pageSize()));
      this.orders.set(response.items ?? []);
      this.totalCount.set(response.totalCount);
      this.totalPages.set(response.totalPages);
      this.currentPage.set(response.pageNumber);
    }catch(error){
       this.logger.error('獲取訂單失敗', error);
    }
  }

  async loadPage(page: number) {
    await this.loadOrders(page);
  }

  constructor() { }
}
