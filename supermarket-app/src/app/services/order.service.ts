import { Injectable, inject, signal, computed, effect } from '@angular/core';
import { OrderApiService } from './order-api.service';
import { OrderRequest} from '../models/order';
import { OrderEntity } from '../models/order';
import { lastValueFrom } from 'rxjs';
import { CartService } from './cart.service';
import { Router, ActivatedRoute } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  private orderApi = inject(OrderApiService);
  private router = inject(Router);
  private cartService = inject(CartService);

 isSubmitting = signal<Boolean>(false);
 currentOrder = signal<OrderEntity | null>(null);
 isProcessing = signal<boolean>(false);
 orders = signal<OrderEntity[]>([]);

  async SubmitOrder(data:OrderRequest){
    this.isSubmitting.set(true);
    try {
      const response = await lastValueFrom(this.orderApi.createOrder(data));
      if (response.success && response?.order?.snowflakeId){
        this.cartService.clearCart();  
        await this.router.navigate(['/order-success'], {
        queryParams: { snowflakeId: response.order.snowflakeId }
      });
      }
    }catch (error: any){
      console.error(error);
    }finally{
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
