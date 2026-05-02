export enum ProductCategory {
  Vegetables = 0,
  Fruits = 1,
  Meat = 2,
  Seafood = 3,
  Dairy = 4,
  Bakery = 5,
  Beverages = 6,
  Snacks = 7,
  Frozen = 8,
  Household = 9,
  PersonalCare = 10,
  Others = 11
}

export interface Product {
  id: number;
  name: string;
  price: number;
  description?: string;
  stockQuantity: number;
  category: ProductCategory;
  photo: string;
  originalPrice?: number;
  brand?: string;
  weight?: number;
  unit?: string;
  isOnSale?: boolean;
  discountPercentage?: number;
}
