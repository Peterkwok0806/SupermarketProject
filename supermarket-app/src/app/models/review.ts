export interface Review {
  id: number;
  userId: number;
  userName: string;
  productId: number;
  productName: string;
  rating: number;
  title?: string;
  content: string;
  isVerifiedPurchase: boolean;
  helpfulCount: number;
  status: ReviewStatus;
  adminReply?: string;
  adminReplyDate?: string;
  images: ReviewImage[];
  createdAt: string;
  updatedAt: string;
  canEdit: boolean;
  hasUserLiked: boolean;
}

export interface ReviewImage {
  id: number;
  imageUrl: string;
  sortOrder: number;
}

export type ReviewStatus = 'Pending' | 'Approved' | 'Rejected' | 'Hidden';

export interface ReviewStats {
  averageRating: number;
  totalReviews: number;
  ratingDistribution: RatingDistribution[];
  verifiedPurchaseCount: number;
}

export interface RatingDistribution {
  rating: number;
  count: number;
  percentage: number;
}

export interface CreateReview {
  productId: number;
  orderId?: number;
  rating: number;
  title?: string;
  content: string;
  imageUrls?: string[];
}

export interface UpdateReview {
  rating: number;
  title?: string;
  content: string;
  imageUrls?: string[];
}

export interface CanReviewResult {
  canReview: boolean;
  reason?: string;
}

export interface AdminReviewFilter {
  status?: ReviewStatus;
  productId?: number;
  rating?: number;
  keyword?: string;
  fromDate?: string;
  toDate?: string;
  page: number;
  pageSize: number;
}

export interface ReviewDashboard {
  totalReviews: number;
  pendingReviews: number;
  approvedReviews: number;
  rejectedReviews: number;
  averageRating: number;
  todayNewReviews: number;
}