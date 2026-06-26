import { Component, OnInit, DestroyRef, inject, signal, computed, effect } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { BaseChartDirective } from 'ng2-charts';
import { forkJoin } from 'rxjs';
import {
  Chart,
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Tooltip,
  Filler,
  Legend,
  Title
} from 'chart.js';
import { OrderStatus } from '../../../models/order';
import { DashboardStats, RecentOrder, LowStockAlert, SalesTrend, SalesTrendPoint } from '../../../models/dashboard';
import { OrderstatusNamePipe } from '../../../pipes/orderstatus-name.pipe';
import { DashboardApiService } from '../../../services/dashboard-api.service';
import { ProductService } from '../../../services/product.service';

// 註冊 Chart.js 所需組件（ng2-charts 需要）
Chart.register(
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Tooltip,
  Filler,
  Legend,
  Title
);

type TrendPeriod = 7 | 14 | 30;

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, OrderstatusNamePipe, MatIconModule, RouterLink, BaseChartDirective],
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

  // 銷售趨勢相關 Signals
  salesTrend = signal<SalesTrend | null>(null);
  isTrendLoading = signal(false);
  trendError = signal<string | null>(null);
  trendPeriod = signal<TrendPeriod>(7);

  // 用於避免 effect() 初次執行時的重複請求（ngOnInit 會主動載入）
  private hasLoadedOnce = signal(false);

  readonly trendPeriods: TrendPeriod[] = [7, 14, 30];

  // 計算屬性：x 軸標籤（取每月日 dd）
  trendLabels = computed(() => {
    const trend = this.salesTrend();
    if (!trend) return [] as string[];
    return trend.points.map(p => p.date.substring(5)); // 取 "MM-dd"
  });

  // 計算屬性：折線圖資料
  trendChartData = computed(() => {
    const trend = this.salesTrend();
    if (!trend) {
      return { labels: [], datasets: [] };
    }
    return {
      labels: this.trendLabels(),
      datasets: [
        {
          label: 'Sales Amount (HKD)',
          data: trend.points.map((p: SalesTrendPoint) => p.salesAmount),
          borderColor: 'rgb(124, 58, 237)',          // 紫色 (purple-600)
          backgroundColor: 'rgba(124, 58, 237, 0.15)',
          pointBackgroundColor: 'rgb(124, 58, 237)',
          pointBorderColor: '#fff',
          pointHoverBackgroundColor: '#fff',
          pointHoverBorderColor: 'rgb(124, 58, 237)',
          pointRadius: 4,
          pointHoverRadius: 6,
          borderWidth: 2.5,
          tension: 0.35,
          fill: true
        }
      ]
    };
  });

  // Chart.js 設定
  trendChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    interaction: {
      mode: 'index' as const,
      intersect: false
    },
    plugins: {
      legend: {
        display: true,
        position: 'top' as const,
        align: 'end' as const,
        labels: {
          color: '#6b7280',
          font: { size: 12, family: 'inherit' },
          boxWidth: 12,
          boxHeight: 12,
          usePointStyle: true,
          pointStyle: 'circle' as const
        }
      },
      tooltip: {
        backgroundColor: 'rgba(17, 24, 39, 0.95)',
        titleColor: '#fff',
        bodyColor: '#e5e7eb',
        borderColor: 'rgba(124, 58, 237, 0.5)',
        borderWidth: 1,
        padding: 12,
        cornerRadius: 8,
        displayColors: true,
        callbacks: {
          label: (ctx: any) => {
            const value = ctx.parsed.y ?? 0;
            return ` Sales: HK$${value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
          }
        }
      }
    },
    scales: {
      x: {
        grid: { display: false },
        ticks: {
          color: '#9ca3af',
          font: { size: 11 }
        }
      },
      y: {
        beginAtZero: true,
        grid: {
          color: 'rgba(0,0,0,0.05)',
          drawBorder: false
        },
        ticks: {
          color: '#9ca3af',
          font: { size: 11 },
          callback: (value: any) => {
            const num = Number(value);
            if (num >= 1000) return `$${(num / 1000).toFixed(1)}k`;
            return `$${num}`;
          }
        }
      }
    }
  };

  // 取得期間內總銷售額 / 訂單數的便捷 computed
  periodTotalSales = computed(() => this.salesTrend()?.totalSales ?? 0);
  periodTotalOrders = computed(() => this.salesTrend()?.totalOrders ?? 0);
  periodAverageSales = computed(() => {
    const t = this.salesTrend();
    if (!t || t.days <= 0) return 0;
    return t.totalSales / t.days;
  });

  constructor() {
    // 當期間變更時自動重新載入圖表資料
    // 使用 hasLoadedOnce 旗標避免 effect 初次執行時與 ngOnInit 重複請求
    effect(() => {
      const days = this.trendPeriod();
      if (this.hasLoadedOnce()) {
        this.loadSalesTrend(days);
      } else {
        this.hasLoadedOnce.set(true);
      }
    });
  }

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
    // 使用 forkJoin 平行載入主要資源以縮短初始載入時間
    this.loadDashboardStats();
    this.loadSalesTrend(this.trendPeriod());
  }

  loadDashboardStats() {
    this.isLoading.set(true);
    // 平行載入：主要 stats + 低庫存警報，縮短初始頁面可見時間
    forkJoin({
      stats: this.dashboardApi.getDashboardStats(),
      lowStock: this.productService.getLowStockAlert()
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ stats, lowStock }) => {
          this.todayOrders.set(stats.todayOrders);
          this.todayRevenue.set(stats.todayRevenue);
          this.totalProducts.set(stats.totalProducts);
          this.totalUsers.set(stats.totalUsers);
          this.pendingOrders.set(stats.pendingOrders);
          this.monthlyRevenue.set(stats.monthlyRevenue);
          this.recentOrders.set(stats.recentOrders);
          this.lowStockAlert.set(lowStock);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load dashboard data:', err);
          this.error.set('Failed to load dashboard data');
          this.isLoading.set(false);
          // 低庫存敗部復活：主要 stats 可能成功但 forkJoin 會一起視為失敗
          // 採取簡單策略：額外單獨取得 lowStock（不影響主要面板）
          this.productService.getLowStockAlert()
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: (alert) => this.lowStockAlert.set(alert),
              error: (e) => console.error('Low stock alert fallback also failed:', e)
            });
        }
      });
  }

  /**
   * 載入最近 N 天的銷售趨勢
   */
  loadSalesTrend(days: TrendPeriod) {
    this.isTrendLoading.set(true);
    this.trendError.set(null);

    this.dashboardApi.getSalesTrend(days).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (trend) => {
        this.salesTrend.set(trend);
        this.isTrendLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load sales trend:', err);
        this.trendError.set('Failed to load sales trend');
        this.isTrendLoading.set(false);
      }
    });
  }

  /**
   * 切換天數區間
   */
  changePeriod(days: TrendPeriod) {
    if (this.trendPeriod() === days) return;
    this.trendPeriod.set(days);
  }
}