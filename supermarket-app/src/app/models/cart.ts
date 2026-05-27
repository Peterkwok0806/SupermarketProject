import { Product } from './product';

export interface Cart {
  id: number;
  userId: number;
  cartItems: CartItemDto[];
  totalAmount:number;
}

export interface CartItemDto {
  cartId: number;
  productId: number;
  product: Product;
  quantity: number;
  unitPrice: number;
  subtotal: number;
  addedAt?: string;
}

export interface CartOperationResult{
  success: Boolean;
  message: String;
  totalAmount: number;
  cart: Cart;
}