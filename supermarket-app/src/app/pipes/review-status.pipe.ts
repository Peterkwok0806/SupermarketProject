import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'reviewStatus',
  standalone: true
})
export class ReviewStatusPipe implements PipeTransform {
  private readonly statusMap: Record<string, string> = {
    'Pending': '待審核',
    'Approved': '已通過',
    'Rejected': '已拒絕',
    'Hidden': '已隱藏'
  };

  transform(value: string): string {
    return this.statusMap[value] || value;
  }
}