import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { OrderEntity } from '../../models/order';
import { lastValueFrom } from 'rxjs';
import { OrderstatusNamePipe } from '../../pipes/orderstatus-name.pipe';
import { OrderStatus } from '../../models/order';
import { BackendImagePipe } from '../../pipes/backend-image.pipe';
import { LoggerService } from '../../services/logger.service';

@Component({
  selector: 'app-order-detail',
  imports: [CommonModule, RouterLink, OrderstatusNamePipe, BackendImagePipe ],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.css'
})
export class OrderDetailComponent implements OnInit{
private route = inject(ActivatedRoute);
private orderServices = inject(OrderService);
private logger = inject(LoggerService);

order =this.orderServices.currentOrder;
isLoading = true;

ngOnInit() {
    const orderSnowflakeId = (this.route.snapshot.paramMap.get('snowflakeId'));
    this.loadOrder(orderSnowflakeId);
  }

async loadOrder(snowflakeId: string| null) {
    this.isLoading = true;
    if (snowflakeId == null){
      return;
    }
    try{
    await this.orderServices.loadOrderDetail(snowflakeId);
    }catch(err){
      this.logger.error('載入訂單失敗', err);
    }finally{
       this.isLoading = false;
    }
  }

  getStatusClass(status: OrderStatus | string): string {
    // Normalize: backend may send string ("Pending") or number (0)
    const s = typeof status === 'string' ? status : OrderStatus[status];
    switch (s) {
      case 'Completed':
        return 'bg-green-100 text-green-800';
      case 'Cancelled':
        return 'bg-red-100 text-red-800';
      case 'Pending':
        return 'bg-amber-100 text-amber-800';
      case 'Paid':
      case 'Processing':
      case 'Shipped':
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }



}
