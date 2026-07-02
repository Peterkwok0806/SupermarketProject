import { Component, OnInit, inject, input, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { ReviewApiService } from '../../../services/review-api.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../services/logger.service';
import { Review, ReviewStats, CanReviewResult } from '../../../models/review';
import { ReviewFormModalComponent } from '../review-form-modal/review-form-modal.component';

@Component({
  selector: 'app-review-section',
  imports: [CommonModule, ReviewFormModalComponent, RouterLink, MatIconModule],
  templateUrl: './review-section.component.html',
  styleUrl: './review-section.component.css'
})
export class ReviewSectionComponent implements OnInit {
  productId = input.required<number>();

  private reviewApi = inject(ReviewApiService);
  private authService = inject(AuthService);
  private logger = inject(LoggerService);

  stats: ReviewStats | null = null;
  reviews: Review[] = [];
  canReviewResult: CanReviewResult | null = null;

  // Filters
  selectedRating: number | null = null;
  selectedSort = 'newest';
  filterHasImage = false;
  filterVerifiedOnly = false;

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;

  // UI state
  isLoading = true;
  isLoadingReviews = false;
  showReviewForm = false;
  editingReview: Review | null = null;

  constructor() {
    // React to productId input changes (e.g., clicking a related product)
    effect(() => {
      const id = this.productId();
      // This effect runs whenever productId signal changes
      this.resetAndLoad(id);
    });
  }

  get isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  get averageRating(): number {
    return this.stats?.averageRating ?? 0;
  }

  get fullStars(): number[] {
    return Array(Math.floor(this.averageRating)).fill(0);
  }

  get hasHalfStar(): boolean {
    return this.averageRating % 1 >= 0.25 && this.averageRating % 1 < 0.75;
  }

  get emptyStars(): number[] {
    const filled = Math.floor(this.averageRating) + (this.hasHalfStar ? 1 : 0);
    return Array(Math.max(0, 5 - filled)).fill(0);
  }

  ngOnInit(): void {
    // Initial load is handled by the effect in constructor
  }

  private resetAndLoad(productId: number): void {
    // Reset filters & pagination for the new product
    this.selectedRating = null;
    this.selectedSort = 'newest';
    this.filterHasImage = false;
    this.filterVerifiedOnly = false;
    this.currentPage = 1;
    this.showReviewForm = false;
    this.editingReview = null;

    this.loadStats();
    this.loadReviews();
    this.checkCanReview();
  }

  loadStats(): void {
    this.reviewApi.getReviewStats(this.productId()).subscribe({
      next: (res) => {
        if (res.success) {
          this.stats = res.item;
        }
      },
      error: (err) => this.logger.error('Failed to load review stats', err)
    });
  }

  loadReviews(): void {
    this.isLoadingReviews = true;
    this.reviewApi.getProductReviews(
      this.productId(),
      this.selectedRating ?? undefined,
      this.filterHasImage || undefined,
      this.filterVerifiedOnly || undefined,
      this.selectedSort,
      this.currentPage,
      this.pageSize
    ).subscribe({
      next: (res) => {
        if (res.success) {
          this.reviews = res.items;
          this.totalCount = res.totalCount;
          this.totalPages = Math.ceil(res.totalCount / this.pageSize);
        }
        this.isLoadingReviews = false;
        this.isLoading = false;
      },
      error: (err) => {
        this.logger.error('Failed to load reviews', err);
        this.isLoadingReviews = false;
        this.isLoading = false;
      }
    });
  }

  checkCanReview(): void {
    if (!this.isLoggedIn) return;
    this.reviewApi.canReview(this.productId()).subscribe({
      next: (res) => {
        if (res.success) {
          this.canReviewResult = res.item;
        }
      }
    });
  }

  selectRatingFilter(rating: number | null): void {
    this.selectedRating = rating;
    this.currentPage = 1;
    this.loadReviews();
  }

  changeSort(sort: string): void {
    this.selectedSort = sort;
    this.currentPage = 1;
    this.loadReviews();
  }

  toggleHasImage(): void {
    this.filterHasImage = !this.filterHasImage;
    this.currentPage = 1;
    this.loadReviews();
  }

  toggleVerifiedOnly(): void {
    this.filterVerifiedOnly = !this.filterVerifiedOnly;
    this.currentPage = 1;
    this.loadReviews();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadReviews();
  }

  getRatingPercentage(rating: number): number {
    if (!this.stats) return 0;
    const dist = this.stats.ratingDistribution.find(d => d.rating === rating);
    return dist?.percentage ?? 0;
  }

  getRatingCount(rating: number): number {
    if (!this.stats) return 0;
    const dist = this.stats.ratingDistribution.find(d => d.rating === rating);
    return dist?.count ?? 0;
  }

  toggleHelpful(review: Review): void {
    this.reviewApi.toggleHelpful(review.id).subscribe({
      next: (res) => {
        if (res.success) {
          review.hasUserLiked = !review.hasUserLiked;
          review.helpfulCount += review.hasUserLiked ? 1 : -1;
        }
      }
    });
  }

  deleteReview(review: Review): void {
    if (!confirm('Are you sure you want to delete this review?')) return;
    this.reviewApi.deleteReview(review.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.loadStats();
          this.loadReviews();
          this.checkCanReview();
        }
      }
    });
  }

  openWriteReview(): void {
    this.editingReview = null;
    this.showReviewForm = true;
  }

  openEditReview(review: Review): void {
    this.editingReview = review;
    this.showReviewForm = true;
  }

  onReviewSubmitted(): void {
    this.showReviewForm = false;
    this.editingReview = null;
    this.loadStats();
    this.loadReviews();
    this.checkCanReview();
  }

  onReviewFormClose(): void {
    this.showReviewForm = false;
    this.editingReview = null;
  }

  getRatingLabel(rating: number): string {
    switch (rating) {
      case 1: return 'Poor';
      case 2: return 'Fair';
      case 3: return 'Good';
      case 4: return 'Very Good';
      case 5: return 'Excellent';
      default: return '';
    }
  }

  getTimeAgo(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    if (diffDays === 0) return 'Today';
    if (diffDays === 1) return 'Yesterday';
    if (diffDays < 30) return `${diffDays} days ago`;
    if (diffDays < 365) return `${Math.floor(diffDays / 30)} months ago`;
    return `${Math.floor(diffDays / 365)} years ago`;
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
}