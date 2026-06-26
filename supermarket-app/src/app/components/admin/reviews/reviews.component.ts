import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { AdminReviewApiService } from '../../../services/admin-review-api.service';
import { Review, ReviewDashboard, ReviewStatus } from '../../../models/review';
import { ReviewStatusPipe } from '../../../pipes/review-status.pipe';

@Component({
  selector: 'app-admin-reviews',
  imports: [CommonModule, FormsModule, ReviewStatusPipe, MatIconModule],
  templateUrl: './reviews.component.html',
  styleUrl: './reviews.component.css'
})
export class AdminReviewsComponent implements OnInit {
  private adminReviewApi = inject(AdminReviewApiService);

  dashboard: ReviewDashboard | null = null;
  reviews: Review[] = [];
  totalCount = 0;
  totalPages = 0;
  currentPage = 1;
  pageSize = 10;

  // Filters
  filterStatus: ReviewStatus | '' = '';
  filterRating: number | null = null;
  filterKeyword = '';

  // UI state
  isLoading = true;
  selectedReview: Review | null = null;
  showDetailModal = false;
  showReplyModal = false;
  replyContent = '';
  isReplying = false;

  ngOnInit(): void {
    this.loadDashboard();
    this.loadReviews();
  }

  loadDashboard(): void {
    this.adminReviewApi.getDashboard().subscribe({
      next: (res) => {
        if (res.success) {
          this.dashboard = res.item;
        }
      }
    });
  }

  loadReviews(): void {
    this.isLoading = true;

    this.adminReviewApi.getReviews(
      this.currentPage,
      this.pageSize,
      this.filterStatus || undefined,
      undefined, // productId
      this.filterRating ?? undefined,
      this.filterKeyword || undefined
    ).subscribe({
      next: (res) => {
        if (res.success) {
          this.reviews = res.items;
          this.totalCount = res.totalCount;
          this.totalPages = Math.ceil(res.totalCount / this.pageSize);
        }
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadReviews();
  }

  clearFilters(): void {
    this.filterStatus = '';
    this.filterRating = null;
    this.filterKeyword = '';
    this.currentPage = 1;
    this.loadReviews();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadReviews();
  }

  approveReview(review: Review): void {
    this.adminReviewApi.updateStatus(review.id, 'Approved').subscribe({
      next: (res) => {
        if (res.success) {
          review.status = 'Approved';
          this.loadDashboard();
        }
      }
    });
  }

  rejectReview(review: Review): void {
    if (!confirm('Are you sure you want to reject this review?')) return;
    this.adminReviewApi.updateStatus(review.id, 'Rejected').subscribe({
      next: (res) => {
        if (res.success) {
          review.status = 'Rejected';
          this.loadDashboard();
        }
      }
    });
  }

  toggleVisibility(review: Review): void {
    const newStatus: ReviewStatus = review.status === 'Hidden' ? 'Approved' : 'Hidden';
    this.adminReviewApi.updateStatus(review.id, newStatus).subscribe({
      next: (res) => {
        if (res.success) {
          review.status = newStatus;
          this.loadDashboard();
        }
      }
    });
  }

  changeStatus(review: Review, newStatus: ReviewStatus): void {
    if (review.status === newStatus) return;
    if (newStatus === 'Rejected' && !confirm('Are you sure you want to reject this review?')) return;
    this.adminReviewApi.updateStatus(review.id, newStatus).subscribe({
      next: (res) => {
        if (res.success) {
          review.status = newStatus;
          this.loadDashboard();
        }
      }
    });
  }

  deleteReview(review: Review): void {
    if (!confirm('Are you sure you want to permanently delete this review?')) return;
    this.adminReviewApi.deleteReview(review.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.loadReviews();
          this.loadDashboard();
        }
      }
    });
  }

  viewDetail(review: Review): void {
    this.selectedReview = review;
    this.showDetailModal = true;
  }

  closeDetailModal(): void {
    this.showDetailModal = false;
    this.selectedReview = null;
  }

  openReplyModal(review: Review): void {
    this.selectedReview = review;
    this.replyContent = review.adminReply || '';
    this.showReplyModal = true;
  }

  closeReplyModal(): void {
    this.showReplyModal = false;
    this.replyContent = '';
  }

  submitReply(): void {
    if (!this.selectedReview || !this.replyContent.trim() || this.isReplying) return;
    this.isReplying = true;

    this.adminReviewApi.replyToReview(this.selectedReview.id, this.replyContent.trim()).subscribe({
      next: (res) => {
        this.isReplying = false;
        if (res.success) {
          this.selectedReview!.adminReply = this.replyContent.trim();
          this.closeReplyModal();
        }
      },
      error: () => this.isReplying = false
    });
  }

  getStatusBadgeClass(status: ReviewStatus): string {
    switch (status) {
      case 'Approved': return 'bg-green-100 text-green-700';
      case 'Pending': return 'bg-yellow-100 text-yellow-700';
      case 'Rejected': return 'bg-red-100 text-red-700';
      case 'Hidden': return 'bg-gray-100 text-gray-600';
      default: return 'bg-gray-100 text-gray-600';
    }
  }

  getStars(rating: number): number[] {
    return Array(rating).fill(0);
  }

  getEmptyStars(rating: number): number[] {
    return Array(5 - rating).fill(0);
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