export interface ProductCategory {
  id: number;
  name: string;        
  description: string; 
  icon: string;        
  displayOrder: number;
}

export interface Product {
  id: number;
  snowflakeId: string;
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
  promotionNames?: string[];

}

export interface ProductDto{
  id: number;
  snowflakeId: string;
  name: string;
  price: number;
  photo: string;
  isOnSale: boolean;
  originalPrice?: number;
  promotionNames?: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

