import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CouponApiService } from '../../services/coupon-api.service';
import {
  CouponListDto,
  CouponUsageDto,
  CouponType,
  formatDiscountDisplay,
  getCouponScopeLabel
} from '../../models/coupon';

@Component({
  selector: 'app-coupons',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './coupons.component.html',
  styleUrl: './coupons.component.css'
})
export class CouponsComponent implements OnInit {
  private couponApi = inject(CouponApiService);

  activeTab: 'available' | 'history' = 'available';

  availableCoupons: CouponListDto[] = [];
  availableLoading = false;
  availableError = '';

  usageHistory: CouponUsageDto[] = [];
  historyLoading = false;
  historyError = '';
  historyPage = 1;
  historyPageSize = 20;
  historyTotalCount = 0;

  CouponType = CouponType;

  ngOnInit(): void {
    this.loadAvailableCoupons();
  }

  switchTab(tab: 'available' | 'history'): void {
    this.activeTab = tab;
    if (tab === 'available' && this.availableCoupons.length === 0) {
      this.loadAvailableCoupons();
    } else if (tab === 'history' && this.usageHistory.length === 0) {
      this.loadUsageHistory();
    }
  }

  loadAvailableCoupons(): void {
    this.availableLoading = true;
    this.availableError = '';
    this.couponApi.getAvailableCoupons().subscribe({
      next: (res) => {
        if (res.success) {
          this.availableCoupons = res.items;
        } else {
          this.availableError = 'Failed to load coupons';
        }
        this.availableLoading = false;
      },
      error: (err) => {
        this.availableError = 'Failed to load coupons. Please try again.';
        this.availableLoading = false;
        console.error('Error loading coupons:', err);
      }
    });
  }

  loadUsageHistory(): void {
    this.historyLoading = true;
    this.historyError = '';
    this.couponApi.getCouponHistory(this.historyPage, this.historyPageSize).subscribe({
      next: (res) => {
        if (res.success) {
          this.usageHistory = res.items;
          this.historyTotalCount = res.totalCount;
        } else {
          this.historyError = 'Failed to load usage history';
        }
        this.historyLoading = false;
      },
      error: (err) => {
        this.historyError = 'Failed to load history. Please try again.';
        this.historyLoading = false;
        console.error('Error loading history:', err);
      }
    });
  }

  getDiscountText(coupon: CouponListDto): string {
    return formatDiscountDisplay(coupon.type, coupon.discountValue, coupon.maximumDiscountAmount);
  }

  getScopeText(coupon: CouponListDto): string {
    return getCouponScopeLabel(coupon.scope);
  }

  getUsageLimitText(coupon: CouponListDto): string {
    if (coupon.usageLimit) {
      return `${coupon.usedCount}/${coupon.usageLimit} used`;
    }
    return `${coupon.usedCount} used`;
  }

  isExpiringSoon(endDate: string): boolean {
    const end = new Date(endDate);
    const now = new Date();
    const diffDays = (end.getTime() - now.getTime()) / (1000 * 60 * 60 * 24);
    return diffDays <= 3 && diffDays > 0;
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getHistoryPages(): number[] {
    const totalPages = Math.ceil(this.historyTotalCount / this.historyPageSize);
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  goToHistoryPage(page: number): void {
    this.historyPage = page;
    this.loadUsageHistory();
  }

  copyCode(code: string, event: Event): void {
    event.stopPropagation();
    navigator.clipboard.writeText(code).then(() => {
      // Could show a toast notification
    });
  }
}