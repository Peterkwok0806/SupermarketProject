import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { AdminCouponApiService } from '../../../services/admin-coupon-api.service';
import { NotificationService } from '../../../services/notification.service';
import {
  CouponListDto,
  CouponStatsDto,
  CouponType,
  CouponScope,
  CreateCouponDto,
  UpdateCouponDto,
  getCouponTypeLabel,
  getCouponScopeLabel,
  formatDiscountDisplay
} from '../../../models/coupon';

@Component({
  selector: 'app-admin-coupons',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './admin-coupons.component.html'
})
export class AdminCouponsComponent implements OnInit {
  private adminCouponApi = inject(AdminCouponApiService);
  private notificationService = inject(NotificationService);

  // Dashboard stats
  stats: CouponStatsDto | null = null;

  // Table data
  coupons: CouponListDto[] = [];
  totalCount = 0;
  totalPages = 0;
  currentPage = 1;
  pageSize = 20;

  // Filters
  filterSearch = '';
  filterType: CouponType | '' = '';
  filterStatus: 'active' | 'expired' | '' = '';
  filterSort = 'newest';

  // UI state
  isLoading = true;
  showModal = false;
  isEditing = false;
  isSaving = false;
  errorMessage = '';

  // Batch operations
  selectedIds = new Set<number>();
  isAllSelected = false;

  // Form model
  form: CreateCouponDto & { id?: number; isActive?: boolean } = this.getEmptyForm();

  // Enum access in template
  CouponType = CouponType;
  CouponScope = CouponScope;

  ngOnInit(): void {
    this.loadStats();
    this.loadCoupons();
  }

  // ===== Data Loading =====

  loadStats(): void {
    this.adminCouponApi.getStats().subscribe({
      next: (res) => {
        if (res.success && res.item) {
          this.stats = res.item;
        }
      }
    });
  }

  loadCoupons(): void {
    this.isLoading = true;
    const isActive = this.filterStatus === 'active' ? true : undefined;
    const isExpired = this.filterStatus === 'expired' ? true : undefined;

    this.adminCouponApi.getCoupons(
      this.currentPage,
      this.pageSize,
      this.filterSearch || undefined,
      this.filterType !== '' ? this.filterType as CouponType : undefined,
      isActive,
      isExpired,
      this.filterSort || undefined
    ).subscribe({
      next: (res) => {
        if (res.success) {
          this.coupons = res.items;
          this.totalCount = res.totalCount;
          this.totalPages = Math.ceil(res.totalCount / this.pageSize);
        }
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  // ===== Filters =====

  applyFilters(): void {
    this.currentPage = 1;
    this.loadCoupons();
  }

  clearFilters(): void {
    this.filterSearch = '';
    this.filterType = '';
    this.filterStatus = '';
    this.filterSort = 'newest';
    this.currentPage = 1;
    this.loadCoupons();
  }

  // ===== Pagination =====

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadCoupons();
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  // ===== CRUD Actions =====

  openCreateModal(): void {
    this.isEditing = false;
    this.form = this.getEmptyForm();
    this.errorMessage = '';
    this.showModal = true;
  }

  openEditModal(coupon: CouponListDto): void {
    this.isEditing = true;
    this.errorMessage = '';
    this.form = {
      id: coupon.id,
      code: coupon.code,
      description: coupon.description || '',
      type: coupon.type,
      discountValue: coupon.discountValue,
      minimumOrderAmount: coupon.minimumOrderAmount || undefined,
      maximumDiscountAmount: coupon.maximumDiscountAmount || undefined,
      usageLimit: coupon.usageLimit || undefined,
      usageLimitPerUser: coupon.usageLimitPerUser || undefined,
      scope: coupon.scope,
      startDate: this.formatDateForInput(coupon.startDate),
      endDate: this.formatDateForInput(coupon.endDate),
      isActive: coupon.isActive,
      productIds: coupon.productIds || [],
      categoryIds: coupon.categoryIds || []
    };
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.errorMessage = '';
  }

  saveCoupon(): void {
    if (this.isSaving) return;

    // Basic validation
    if (!this.form.code?.trim()) {
      this.errorMessage = 'Coupon code is required';
      return;
    }
    if (!this.form.discountValue || this.form.discountValue <= 0) {
      this.errorMessage = 'Discount value must be greater than 0';
      return;
    }
    if (!this.form.startDate || !this.form.endDate) {
      this.errorMessage = 'Start and end dates are required';
      return;
    }
    if (new Date(this.form.endDate) <= new Date(this.form.startDate)) {
      this.errorMessage = 'End date must be after start date';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    if (this.isEditing && this.form.id) {
      const dto: UpdateCouponDto = {
        ...this.form as CreateCouponDto,
        id: this.form.id,
        isActive: this.form.isActive ?? true
      };
      this.adminCouponApi.updateCoupon(dto).subscribe({
        next: (res) => {
          this.isSaving = false;
          if (res.success) {
            this.closeModal();
            this.loadCoupons();
            this.loadStats();
          } else {
            this.errorMessage = res.message || 'Failed to update coupon';
          }
        },
        error: (err) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || 'Failed to update coupon';
        }
      });
    } else {
      this.adminCouponApi.createCoupon(this.form as CreateCouponDto).subscribe({
        next: (res) => {
          this.isSaving = false;
          if (res.success) {
            this.closeModal();
            this.loadCoupons();
            this.loadStats();
          } else {
            this.errorMessage = res.message || 'Failed to create coupon';
          }
        },
        error: (err) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || 'Failed to create coupon';
        }
      });
    }
  }

  toggleActive(coupon: CouponListDto): void {
    this.adminCouponApi.toggleActive(coupon.id).subscribe({
      next: (res) => {
        if (res.success) {
          // Use server response value instead of local toggle
          coupon.isActive = res.item ?? !coupon.isActive;
          this.loadStats();
        }
      }
    });
  }

  deleteCoupon(coupon: CouponListDto): void {
    if (!confirm(`Are you sure you want to delete coupon "${coupon.code}"?`)) return;
    this.adminCouponApi.deleteCoupon(coupon.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.loadCoupons();
          this.loadStats();
        }
      }
    });
  }

  // ===== Display Helpers =====

  getTypeLabel(type: CouponType): string {
    return getCouponTypeLabel(type);
  }

  getScopeLabel(scope: CouponScope): string {
    return getCouponScopeLabel(scope);
  }

  getDiscountDisplay(coupon: CouponListDto): string {
    return formatDiscountDisplay(coupon.type, coupon.discountValue, coupon.maximumDiscountAmount);
  }

  getUsageDisplay(coupon: CouponListDto): string {
    const limit = coupon.usageLimit ? coupon.usageLimit.toString() : '∞';
    return `${coupon.usedCount} / ${limit}`;
  }

  isExpired(endDate: string): boolean {
    return new Date(endDate) < new Date();
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  formatDateForInput(dateStr: string): string {
    const d = new Date(dateStr);
    // Use local time for datetime-local input instead of UTC (toISOString)
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  getUsagePercent(coupon: CouponListDto): number {
    if (!coupon.usageLimit || coupon.usageLimit === 0) return 0;
    return Math.min(100, Math.round((coupon.usedCount / coupon.usageLimit) * 100));
  }

  // ===== Batch Operations =====

  toggleSelect(id: number): void {
    if (this.selectedIds.has(id)) {
      this.selectedIds.delete(id);
    } else {
      this.selectedIds.add(id);
    }
    this.isAllSelected = this.coupons.length > 0 && this.coupons.every(c => this.selectedIds.has(c.id));
  }

  toggleSelectAll(): void {
    if (this.isAllSelected) {
      this.selectedIds.clear();
      this.isAllSelected = false;
    } else {
      this.coupons.forEach(c => this.selectedIds.add(c.id));
      this.isAllSelected = true;
    }
  }

  isItemSelected(id: number): boolean {
    return this.selectedIds.has(id);
  }

  clearSelection(): void {
    this.selectedIds.clear();
    this.isAllSelected = false;
  }

  bulkDeleteSelected(): void {
    const ids = Array.from(this.selectedIds);
    if (ids.length === 0) return;
    if (!confirm(`Are you sure you want to delete ${ids.length} coupon(s)?`)) return;

    this.adminCouponApi.bulkDelete(ids).subscribe({
      next: (res) => {
        const deleted = res.deleted ?? 0;
        const errors: string[] = res.errors ?? [];
        if (errors.length > 0) {
          this.notificationService.error(`Deleted ${deleted} coupon(s).\nSkipped:\n${errors.join('\n')}`);
        } else {
          this.notificationService.success(`Successfully deleted ${deleted} coupon(s).`);
        }
        this.clearSelection();
        this.loadCoupons();
        this.loadStats();
      },
      error: () => {
        this.notificationService.error('Failed to bulk delete coupons.');
      }
    });
  }

  private getEmptyForm(): CreateCouponDto & { id?: number; isActive?: boolean } {
    return {
      code: '',
      description: '',
      type: CouponType.Percentage,
      discountValue: 0,
      minimumOrderAmount: undefined,
      maximumDiscountAmount: undefined,
      usageLimit: undefined,
      usageLimitPerUser: undefined,
      scope: CouponScope.Global,
      startDate: '',
      endDate: '',
      isActive: true,
      productIds: [],
      categoryIds: []
    };
  }
}