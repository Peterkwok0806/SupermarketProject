import { Component, OnInit, inject } from '@angular/core';
import { Product } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { CategoryNamePipe } from '../../pipes/category-name.pipe';


@Component({
  selector: 'app-productlist',
  imports: [CommonModule, CategoryNamePipe],
  templateUrl: './productlist.component.html',
  styleUrl: './productlist.component.css'
})
export class ProductlistComponent implements OnInit{

  private productService = inject(ProductService);

  products$!: Observable<Product[]>;

  ngOnInit(): void {
      this.products$ = this.productService.getProducts();
    }

  
}
