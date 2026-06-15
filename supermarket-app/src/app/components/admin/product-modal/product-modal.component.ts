import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../services/product.service';
import { Product} from '../../../models/product';
import { lastValueFrom } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import { NotificationService } from '../../../services/notification.service';

@Component({
  selector: 'app-product-modal',
  imports: [CommonModule, FormsModule, ],
  templateUrl: './product-modal.component.html',
  styleUrl: './product-modal.component.css'
})
export class ProductModalComponent {
  private productService = inject(ProductService);
  private notification = inject(NotificationService);

  product:Product |null = null;

  // 1. 接收 ID
  @Input() productId: number | null = null; 
  @Output() closeModal = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  isEditMode = false;
  categories: any[] = [];
  isLoading = false;
  previewUrl: string | null = null;
  selectedFile: File | null = null;

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

  async saveProduct() {
    if (!this.productData.name || !this.productData.categoryId || this.productData.price <= 0) {
      this.notification.error('請填寫必要欄位');
      return;
    }
      // 建立 FormData 物件
    const formData = new FormData();
    formData.append('name', this.productData.name);
    formData.append('description', this.productData.description);
    formData.append('price', this.productData.price.toString());
    formData.append('categoryId', this.productData.categoryId.toString());
    formData.append('stockQuantity', this.productData.stockQuantity.toString());
  
      // 2. 把圖片實體塞進去（注意：這裡的 key 'Photo' 必須對應後端 CreateProductDto 的屬性名稱）
    if (this.selectedFile) {
      formData.append('photofile', this.selectedFile);
    }

    try{
    if (this.isEditMode && this.product) {
      const response = await firstValueFrom(this.productService.updateProduct(this.product.id,formData))
      console.log(response);
      this.notification.success('商品更新成功');
     } 
    else {
      const response = await firstValueFrom (this.productService.createProduct(formData))
      console.log(response);
      this.notification.success('新增商品成功');
    }
    this.saved.emit();
    this.close();
    }catch (error) {
      this.notification.error('儲存失敗，請稍後再試');
    }
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      this.previewUrl = URL.createObjectURL(file);
    }
  }


  close() {
    this.closeModal.emit();
  }

}
