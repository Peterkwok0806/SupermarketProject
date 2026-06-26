export interface DashboardStats {
  todayOrders: number;
  todayRevenue: number;
  totalProducts: number;
  totalUsers: number;
  pendingOrders: number;
  monthlyRevenue: number;
  recentOrders: RecentOrder[];
}

export interface RecentOrder {
  snowflakeId: string;
  fullName: string;
  totalAmount: number;
  status: number;
  createdAt: Date;
}

export interface LowStockProduct {
  id: number;
  name: string;
  stockQuantity: number;
}

export interface LowStockAlert {
  totalLowStockCount: number;
  threshold: number;
  lowStockProducts: LowStockProduct[];
}

/**
 * 銷售趨勢：每日資料點（對應後端 SalesTrendPoint）
 */
export interface SalesTrendPoint {
  date: string;          // yyyy-MM-dd
  salesAmount: number;   // 當日銷售額
  orderCount: number;    // 當日訂單數
}

/**
 * 銷售趨勢匯總（對應後端 SalesTrendDto）
 */
export interface SalesTrend {
  days: number;          // 查詢天數 (7 / 14 / 30)
  startDate: string;     // yyyy-MM-dd
  endDate: string;       // yyyy-MM-dd
  totalSales: number;    // 期間總銷售額
  totalOrders: number;   // 期間總訂單數
  points: SalesTrendPoint[]; // 連續日期資料（已補齊零銷量日）
}