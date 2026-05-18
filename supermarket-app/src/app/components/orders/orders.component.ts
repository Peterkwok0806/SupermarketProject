import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { OrderstatusNamePipe } from '../../pipes/orderstatus-name.pipe';
import { OrderStatus } from '../../models/order';

@Component({
  selector: 'app-orders',
  imports: [CommonModule, OrderstatusNamePipe, RouterModule],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class OrdersComponent implements OnInit {
protected orderService = inject(OrderService);

ngOnInit() {
    this.orderService.loadOrders();
  }

get orders() {
    return this.orderService.orders();
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
