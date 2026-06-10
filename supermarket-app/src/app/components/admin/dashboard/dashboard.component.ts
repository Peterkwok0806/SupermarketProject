import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderStatus } from '../../../models/order';
import { OrderstatusNamePipe } from '../../../pipes/orderstatus-name.pipe';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule,OrderstatusNamePipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit{
  todayOrders = 12;
  totalRevenue = 45890;
  totalProducts = 245;
  totalUsers = 1894;

  recentOrders = [
    { id: 1001, totalAmount: 289.5, status: 4, createdAt: new Date() },
    { id: 1002, totalAmount: 156.0, status: 3, createdAt: new Date(Date.now() - 100000000) },
    { id: 1003, totalAmount: 89.9, status: 2, createdAt: new Date(Date.now() - 200000000) },
  ];

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

  ngOnInit() {
    // 之後會從後端 API 取得真實數據
  }
}
