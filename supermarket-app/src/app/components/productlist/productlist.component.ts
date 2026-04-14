import { Component, OnInit, inject } from '@angular/core';
import { Product } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';


@Component({
  selector: 'app-productlist',
  imports: [CommonModule],
  template:`
  <h2>🛒 超市商品清單 (Inline Template)</h2>
  @for (item of (products$ | async); track item.id){
   <h3>{{ item.name }}</h3>
   <p>價格：{{ item.price | currency:'HKD':'symbol' }}</p>
  }@empty {
          <p>⚠️ 正在載入資料或目前沒有商品...</p>
    }
  `,
  styleUrl: './productlist.component.css'
})
export class ProductlistComponent implements OnInit{

  private productService = inject(ProductService);

  products$!: Observable<Product[]>;



ngOnInit(): void {
    this.products$ = this.productService.getProducts();
  }
}
