import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { OrderService } from '../../services/order.service';



@Component({
  selector: 'app-order-success',
  imports: [CommonModule, RouterLink],
  templateUrl: './order-success.component.html',
  styleUrl: './order-success.component.css'
})
export class OrderSuccessComponent implements OnInit{
  private route = inject(ActivatedRoute); 
  protected orderService = inject(OrderService);

 
  ngOnInit() {
    this.route.queryParams.subscribe(async params => {
      const snowflakeId = params['snowflakeId'];
      if (snowflakeId) {
        // 通知邏輯層去撈取資料
        await this.orderService.loadOrderDetail(snowflakeId);
      }
    });
  }
  

  handleReload() {
  window.location.reload();
}
             
}
