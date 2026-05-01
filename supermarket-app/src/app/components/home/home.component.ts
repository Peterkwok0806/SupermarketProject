import { Component } from '@angular/core';
import { ProductlistComponent } from '../productlist/productlist.component';

@Component({
  selector: 'app-home',
  imports: [ProductlistComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {

}
