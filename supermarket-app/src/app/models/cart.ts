import { Product } from './product';

export interface Cart {
  id: number;
  userId: number;
  cartItems: CartItemDto[];
}

export interface CartItemDto {
  cartId: number;
  productId: number;
  product: Product;
  quantity: number;
  unitPrice: number;
  addedAt?: string;
}