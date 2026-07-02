import { Injectable, inject, signal, computed, effect } from '@angular/core';
import { WishlistApiService } from './wishlist-api.service';
import { ProductDto } from '../models/product';
import { AuthService } from './auth.service';
import { NotificationService } from './notification.service';
import { LoggerService } from './logger.service';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private wishlistApi = inject(WishlistApiService);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private logger = inject(LoggerService);

  // 所有收藏的商品 ID 集合（用於快速判斷是否已收藏）
  private _wishlistProductIds = signal<Set<number>>(new Set());

  // 所有收藏的商品資料
  private _wishlistProducts = signal<ProductDto[]>([]);

  readonly wishlistProducts = this._wishlistProducts.asReadonly();

  isLoading = signal<boolean>(false);

  // 收藏總數
  totalItems = computed(() => this._wishlistProductIds().size);

  constructor() {
    effect(() => {
      if (this.authService.isLoggedIn()) {
        this.loadWishlist();
      } else {
        this.resetWishlist();
      }
    }, { allowSignalWrites: true });
  }

  /**
   * 載入願望清單
   */
  async loadWishlist() {
    try {
      const response = await firstValueFrom(this.wishlistApi.getWishlist());
      if (response.success && response.item) {
        const ids = new Set(response.item.map(p => p.id));
        this._wishlistProductIds.set(ids);
        this._wishlistProducts.set(response.item);
      }
    } catch (err) {
      this.logger.error('無法取得願望清單', err);
    }
  }

  /**
   * 檢查某商品是否在願望清單中
   */
  isInWishlist(productId: number): boolean {
    return this._wishlistProductIds().has(productId);
  }

  /**
   * 切換收藏狀態（加入 / 取消）
   */
  async toggleWishlist(productId: number): Promise<void> {
    this.isLoading.set(true);
    try {
      if (this.isInWishlist(productId)) {
        // 已收藏 → 取消收藏
        const result = await firstValueFrom(this.wishlistApi.removeFromWishlist(productId));
        if (result.success) {
          this._wishlistProductIds.update(ids => {
            const newIds = new Set(ids);
            newIds.delete(productId);
            return newIds;
          });
          // 同步更新 products 列表
          this._wishlistProducts.update(products =>
            products.filter(p => p.id !== productId)
          );
          this.notificationService.success('已取消收藏 ❤️');
        }
      } else {
        // 未收藏 → 加入收藏
        const result = await firstValueFrom(this.wishlistApi.addToWishlist(productId));
        if (result.success) {
          this._wishlistProductIds.update(ids => {
            const newIds = new Set(ids);
            newIds.add(productId);
            return newIds;
          });
          this.notificationService.success('已加入願望清單 ❤️');
        } else {
          this.notificationService.error(result.message || '加入願望清單失敗');
        }
      }
    } catch (error: any) {
      const msg = error?.error?.message || error?.message || '操作失敗，請稍後再試';
      this.logger.error('切換願望清單失敗', error);
      this.notificationService.error(msg);
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * 從願望清單中移除商品（用於願望清單頁面）
   */
  async removeFromWishlist(productId: number): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await firstValueFrom(this.wishlistApi.removeFromWishlist(productId));
      if (result.success) {
        this._wishlistProductIds.update(ids => {
          const newIds = new Set(ids);
          newIds.delete(productId);
          return newIds;
        });
        this._wishlistProducts.update(products =>
          products.filter(p => p.id !== productId)
        );
        this.notificationService.success('已從願望清單中移除');
      }
    } catch (error: any) {
      const msg = error?.error?.message || error?.message || '移除失敗，請稍後再試';
      this.logger.error('移除願望清單失敗', error);
      this.notificationService.error(msg);
    } finally {
      this.isLoading.set(false);
    }
  }

  resetWishlist() {
    this._wishlistProductIds.set(new Set());
    this._wishlistProducts.set([]);
  }
}
