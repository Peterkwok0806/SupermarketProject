export interface WishlistItem {
  id: number;
  userId: number;
  productId: number;
  createdAt: string;
}

export interface WishlistOperationResult {
  success: boolean;
  message: string;
}
