import { Pipe, PipeTransform } from '@angular/core';
import { ProductCategory } from '../models/product';

@Pipe({
  name: 'categoryName'
})
export class CategoryNamePipe implements PipeTransform {

  private readonly CATEGORY_LABEL_MAP: Record<ProductCategory, string> = {
  [ProductCategory.Vegetables]: 'Vegetables',
  [ProductCategory.Fruits]: 'Fruits',
  [ProductCategory.Meat]: 'Meat',
  [ProductCategory.Seafood]: 'Seafood',
  [ProductCategory.Dairy]: 'Dairy',
  [ProductCategory.Bakery]: 'Bakery',
  [ProductCategory.Beverages]: 'Beverages',
  [ProductCategory.Snacks]: 'Snacks',
  [ProductCategory.Frozen]: 'Frozen',
  [ProductCategory.Household]: 'Household',
  [ProductCategory.PersonalCare]: 'PersonalCare',
  [ProductCategory.Others]: 'Others',
};

  transform(value: ProductCategory | number): string {
    return this.CATEGORY_LABEL_MAP[value as ProductCategory] ?? 'Others';
  }
}
