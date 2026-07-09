import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { OrderstatusNamePipe } from '../../pipes/orderstatus-name.pipe';
import { OrderStatus } from '../../models/order';
import { SkeletonComponent } from '../../shared/skeleton/skeleton.component';

@Component({
  selector: 'app-orders',
  imports: [CommonModule, OrderstatusNamePipe, MatTableModule, MatButtonModule, MatIconModule, RouterModule, SkeletonComponent],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class OrdersComponent implements OnInit {
protected orderService = inject(OrderService);
isLoading = true;

displayedColumns: string[] = ['id', 'date', 'status', 'totalAmount', 'action'];

async ngOnInit(): Promise<void> {
    this.isLoading = true;
    await this.orderService.loadOrders();
    this.isLoading = false;
  }

get orders() {
    return this.orderService.orders();
  }

get currentPage() { return this.orderService.currentPage(); }
get totalPages() { return this.orderService.totalPages(); }
get totalCount() { return this.orderService.totalCount(); }
get hasPages() { return this.totalPages > 1; }

onPageChange(page: number) {
    if (page < 1 || page > this.totalPages) return;
    this.orderService.loadPage(page);
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
