import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { OrderStatus} from '../../../models/order';
import { OrderstatusNamePipe } from '../../../pipes/orderstatus-name.pipe';

@Component({
  selector: 'app-statusupdate-modal',
  imports: [CommonModule,FormsModule,OrderstatusNamePipe],
  templateUrl: './statusupdate-modal.component.html',
  styleUrl: './statusupdate-modal.component.css'
})
export class StatusupdateModalComponent {
  @Input() order: any | null = null;
  
  // 可選：由父組件告知目前 API 是否正在載入中（防止重複點擊）
  @Input() isSubmitting: boolean = false;

  // 事件發送：關閉視窗、儲存變更
  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<OrderStatus>();

  protected readonly OrderStatus = OrderStatus;
  tempStatus: OrderStatus = OrderStatus.Pending;

  statusOptions = Object.values(OrderStatus)
    .filter((v): v is OrderStatus => typeof v === 'number');

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['order'] && this.order) {
      this.tempStatus = Number(this.order.status) as OrderStatus;
    }
  }

  onCancel(): void {
    if (this.isSubmitting) return; // 傳送中禁止關閉
    this.close.emit();
  }

  onConfirm(): void {
    // 只有在狀態有變更，且非傳送中時才觸發
    if (this.tempStatus && this.tempStatus !== this.order?.status && !this.isSubmitting) {
      this.save.emit(this.tempStatus);
    }
  }
}
