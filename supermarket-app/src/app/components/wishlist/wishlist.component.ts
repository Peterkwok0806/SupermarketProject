import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { WishlistService } from '../../services/wishlist.service';
import { NotificationService } from '../../services/notification.service';
import { LoggerService } from '../../services/logger.service';
import { BackendImagePipe } from '../../pipes/backend-image.pipe';

@Component({
  selector: 'app-wishlist',
  imports: [CommonModule, RouterLink, BackendImagePipe],
  templateUrl: './wishlist.component.html',
  styleUrl: './wishlist.component.css'
})
export class WishlistComponent {
  private wishlistService = inject(WishlistService);
  private notificationService = inject(NotificationService);
  private logger = inject(LoggerService);

  wishlistProducts = this.wishlistService.wishlistProducts;
  isLoading = this.wishlistService.isLoading;

  totalCount = computed(() => this.wishlistService.totalItems());
  isEmpty = computed(() => this.wishlistProducts().length === 0 && !this.isLoading());

  async removeFromWishlist(productId: number) {
    try {
      await this.wishlistService.removeFromWishlist(productId);
    } catch (error) {
      this.logger.error('從願望清單移除失敗', error);
      this.notificationService.error('移除失敗，請稍後再試');
    }
  }
}
