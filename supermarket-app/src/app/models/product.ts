export interface ProductCategory {
  id: number;
  name: string;        
  description: string; 
  icon: string;        
  displayOrder: number;
}

export interface Product {
  id: number;
  name: string;
  price: number;
  description?: string;
  stockQuantity: number;
  categoryId: number;
  category: ProductCategory;
  photo: string;
  originalPrice?: number;
  brand?: string;
  weight?: number;
  unit?: string;
  isOnSale?: boolean;
  discountPercentage?: number;
}

export interface ProductDto{
  id: number;
  name: string;
  price: number;
  photo: string;
}
