import { Component } from '@angular/core';
import { ProductlistComponent } from '../productlist/productlist.component';
import { BannerComponent } from '../banner/banner.component';

@Component({
  selector: 'app-home',
  imports: [ProductlistComponent, BannerComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {

}
