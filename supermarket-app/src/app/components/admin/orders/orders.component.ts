import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderApiService } from '../../../services/order-api.service';
import { OrderStatus } from '../../../models/order';
import { OrderstatusNamePipe } from '../../../pipes/orderstatus-name.pipe';

@Component({
  selector: 'app-orders',
  imports: [CommonModule,OrderstatusNamePipe],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class AdminOrdersComponent implements OnInit{
  private orderApi = inject(OrderApiService);

  orders: any[] = [];

  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    
  }

  viewOrderDetail(order: any) {
    alert(`查看訂單 #${order.id} 詳細資訊 (功能開發中)`);
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
