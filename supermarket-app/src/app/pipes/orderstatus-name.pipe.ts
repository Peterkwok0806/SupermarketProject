import { Pipe, PipeTransform } from '@angular/core';
import { OrderStatus} from '../models/order';

@Pipe({
  name: 'orderstatusName'
})
export class OrderstatusNamePipe implements PipeTransform {

   private readonly CATEGORY_LABEL_MAP: Record<OrderStatus, string> = {
    [OrderStatus.Pending]: 'Pending',
    [OrderStatus.Paid]: 'Paid',
    [OrderStatus.Processing]: 'Processing',
    [OrderStatus.Shipped]: 'Shipped',
    [OrderStatus.Completed]: 'Completed',
    [OrderStatus.Cancelled]: 'Cancelled',
    };

  transform(value: OrderStatus | number | string): string {
    return this.CATEGORY_LABEL_MAP[value as OrderStatus]|| '未知狀態';
  }

}
