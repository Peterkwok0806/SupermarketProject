import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { OrderEntity } from '../../models/order';
import { lastValueFrom } from 'rxjs';
import { OrderstatusNamePipe } from '../../pipes/orderstatus-name.pipe';
import { OrderStatus } from '../../models/order';

@Component({
  selector: 'app-order-detail',
  imports: [CommonModule, RouterLink, OrderstatusNamePipe ],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.css'
})
export class OrderDetailComponent implements OnInit{
private route = inject(ActivatedRoute);
private orderServices = inject(OrderService);

order =this.orderServices.currentOrder;
isLoading = true;

ngOnInit() {
    const orderId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadOrder(orderId);
  }

async loadOrder(orderId: number) {
    this.isLoading = true;
    try{
    await this.orderServices.loadOrderDetail(orderId);
    }catch(err){
      console.error(err);
    }finally{
       this.isLoading = false;
    }
  }

getStatusClass(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Completed:
        return 'bg-green-100 text-green-800'; // 綠色
        
      case OrderStatus.Cancelled:
        return 'bg-red-100 text-red-800'; // 紅色
        
      case OrderStatus.Pending:
        return 'bg-amber-100 text-amber-800'; // 琥珀色/深黃（待付款）
        
      case OrderStatus.Paid:
      case OrderStatus.Processing:
      case OrderStatus.Shipped:
        return 'bg-blue-100 text-blue-800'; // 藍色（處理中系列）
        
      default:
        return 'bg-gray-100 text-gray-800'; // 灰色
    }
  }



}
