import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../../services/product.service';

@Component({
  selector: 'app-products',
  imports: [CommonModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class AdminProductsComponent implements OnInit{
  private productService = inject(ProductService);

  products: any[] = [];

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    
  }

  editProduct(product: any) {
    alert(`編輯商品: ${product.name} (功能開發中)`);
  }

  toggleAvailability(product: any) {
    alert(`切換 ${product.name} 上下架狀態 (功能開發中)`);
  }

  openAddProductModal() {
    alert("新增商品功能開發中...");
  }
}
