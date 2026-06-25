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