import { Pipe, PipeTransform } from '@angular/core';
import { OrderStatus} from '../models/order';

@Pipe({
  name: 'orderstatusName'
})
export class OrderstatusNamePipe implements PipeTransform {

  private readonly STATUS_LABEL_MAP: Record<string, string> = {
    'Pending': 'Pending',
    'Paid': 'Paid',
    'Processing': 'Processing',
    'Shipped': 'Shipped',
    'Completed': 'Completed',
    'Cancelled': 'Cancelled',
  };

  transform(value: OrderStatus | number | string): string {
    // Handle string enum values from backend (e.g., "Pending", "Paid")
    if (typeof value === 'string' && this.STATUS_LABEL_MAP[value]) {
      return this.STATUS_LABEL_MAP[value];
    }
    // Fallback for numeric enum values
    const enumName = OrderStatus[value as number];
    return this.STATUS_LABEL_MAP[enumName] || '未知狀態';
  }

}
