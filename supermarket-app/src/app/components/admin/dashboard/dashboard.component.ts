import { Component, OnInit, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { OrderStatus } from '../../../models/order';
import { DashboardStats, RecentOrder, LowStockAlert } from '../../../models/dashboard';
import { OrderstatusNamePipe } from '../../../pipes/orderstatus-name.pipe';
import { DashboardApiService } from '../../../services/dashboard-api.service';
import { ProductService } from '../../../services/product.service';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, OrderstatusNamePipe, MatIconModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  private dashboardApi = inject(DashboardApiService);
  private productService = inject(ProductService);
  private destroyRef = inject(DestroyRef);

  isLoading = signal(true);
  error = signal<string | null>(null);

  todayOrders = signal(0);
  todayRevenue = signal(0);
  totalProducts = signal(0);
  totalUsers = signal(0);
  pendingOrders = signal(0);
  monthlyRevenue = signal(0);
  recentOrders = signal<RecentOrder[]>([]);
  lowStockAlert = signal<LowStockAlert | null>(null);

  getStatusClass(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Completed:
        return 'bg-green-100 text-green-800';
      case OrderStatus.Cancelled:
        return 'bg-red-100 text-red-800';
      case OrderStatus.Pending:
        return 'bg-amber-100 text-amber-800';
      case OrderStatus.Paid:
      case OrderStatus.Processing:
      case OrderStatus.Shipped:
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  ngOnInit() {
    this.loadDashboardStats();
  }

  loadDashboardStats() {
    this.isLoading.set(true);
    this.dashboardApi.getDashboardStats().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (stats: DashboardStats) => {
        this.todayOrders.set(stats.todayOrders);
        this.todayRevenue.set(stats.todayRevenue);
        this.totalProducts.set(stats.totalProducts);
        this.totalUsers.set(stats.totalUsers);
        this.pendingOrders.set(stats.pendingOrders);
        this.monthlyRevenue.set(stats.monthlyRevenue);
        this.recentOrders.set(stats.recentOrders);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load dashboard stats:', err);
        this.error.set('Failed to load dashboard data');
        this.isLoading.set(false);
      }
    });

    // 低庫存警報（獨立請求，失敗不影響主面板）
    this.productService.getLowStockAlert().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (alert) => this.lowStockAlert.set(alert),
      error: (err) => console.error('Failed to load low stock alert:', err)
    });
  }
}