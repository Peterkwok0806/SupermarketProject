import { Injectable, inject, signal, computed, effect } from '@angular/core';
import { OrderApiService } from './order-api.service';
import { OrderRequest} from '../models/order';
import { Product } from '../models/product';
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

  async SubmitOrder(data:OrderRequest){
    this.isSubmitting.set(true);
    try {
      const response = await lastValueFrom(this.orderApi.createOrder(data));
      console.log(response);
      if (response.success){
        alert(`🎉 訂單建立成功！`);
        this.cartService.clearCart();
        this.router.navigate(['/']);
      }
    }catch (error: any){
      console.error(error);
    }finally{
      this.isSubmitting.set(false);
    }
  }

  constructor() { }
}
