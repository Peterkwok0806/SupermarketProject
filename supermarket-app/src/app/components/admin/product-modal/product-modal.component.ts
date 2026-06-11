import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../services/product.service';
import { Product} from '../../../models/product';
import { Subscription,lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-product-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './product-modal.component.html',
  styleUrl: './product-modal.component.css'
})
export class ProductModalComponent {
  private productService = inject(ProductService);

  // 1. 接收 ID
  @Input() productId: number | null = null; 
  @Output() closeModal = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  product: Product | null = null;
  isEditMode = false;
  categories: any[] = [];
  isLoading = false;

  productData = {
    name: '',
    price: 0,
    stockQuantity: 0,
    categoryId: 0,
    photo: '',
    description: ''
  };

  ngOnInit() {
    this.loadCategories();
    
    //判斷是否有傳入 ID 決定是否為編輯模式
    if (this.productId) {
      this.isEditMode = true;
      this.loadProductDetails(this.productId);
    }else{
      this.isEditMode = false;
      
    }
  }

  loadCategories() {
    this.productService.getCategories().subscribe(data => this.categories = data);
  }

  async loadProductDetails(id: number) {
    this.isLoading = true;
    try{
       this.product = await lastValueFrom(this.productService.getProductById(id))
       if (this.product) {
        // 將 API 回傳的資料同步賦值給表單綁定的物件
          this.productData = {
            name: this.product.name || '',
            price: this.product.price || 0,
            stockQuantity: this.product.stockQuantity || 0,
            categoryId: this.product.categoryId ?? undefined, // 確保對應到選單的 null
            photo: this.product.photo || '',
            description: this.product.description || ''
          }; 
        }
    }catch(err){
      console.error('載入商品詳情失敗', err);
    }finally{
      this.isLoading = false;
    }
  }

  saveProduct() {
    if (this.isEditMode) {
      // TODO: 呼叫更新 API
      console.log('更新商品', this.productData);
    } else {
      // TODO: 呼叫新增 API
      console.log('新增商品', this.productData);
    }
    this.saved.emit();
    this.close();
  }

  close() {
    this.closeModal.emit();
  }
}
