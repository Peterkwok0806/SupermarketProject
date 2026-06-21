import { Component,inject,signal, computed, resource } from '@angular/core';
import { toSignal, } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, FormGroup, ValidatorFn, AbstractControl } from '@angular/forms'; 
import { OrderApiService } from '../../../services/order-api.service';
import { OrderStatus } from '../../../models/order';
import { OrderstatusNamePipe } from '../../../pipes/orderstatus-name.pipe';
import { RouterLink, ActivatedRoute, Router} from '@angular/router'; 
import { map,firstValueFrom} from 'rxjs';
import {searchOrderRequest} from '../../../models/order';
import { StatusupdateModalComponent } from '../statusupdate-modal/statusupdate-modal.component'


@Component({
  selector: 'app-orders',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, OrderstatusNamePipe,StatusupdateModalComponent ],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class AdminOrdersComponent {
  private fb = inject(FormBuilder);
  private orderApi = inject(OrderApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  editingOrder: any | null = null;
  isModalLoading: boolean = false;

  // 搜尋表單暫存欄位，用作 HTML 的雙向綁定
  searchForm = this.fb.group({
    orderId: [null as string | null],
    userName: [null as string | null],
    startDate: [null as Date | null],
    endDate: [null as Date | null]
  },{
    validators: [this.dateRangeValidator()]
  }
);

 
   // 真正送出查詢的 Signal 條件
  searchFilters = signal<Partial<searchOrderRequest>>({});

  pageSize = signal<number>(10);

   // 從網址追蹤目前頁碼
  currentPage = toSignal(
    this.route.queryParams.pipe(
      map(params => {
        const page = params['page'] ? Number(params['page']) : 1;
        return page < 1 ? 1 : page; 
      })
    ),
    { initialValue: 1 }
  );

  // 當 filters、page 或 size 改變時會自動觸發 loader
   OrderResource = resource({
    // 只要這裡定義的變數（Signal）改變，就會自動觸發下面的 loader
    request: () => ({
      filters: this.searchFilters(),
      page: this.currentPage(),
      size: this.pageSize()
    }),
    // 執行異步請求（底層必須回傳 Promise，所以用 firstValueFrom 轉換，或改用 http.get 的 Promise 版本）
    loader: async ({ request }) => {
     
      const result = await firstValueFrom(
        this.orderApi.searchOrders(request.filters as searchOrderRequest,request.page, request.size)
      );
      return result || { items: [], totalPages: 0 };
    }
  });

  // 唯讀資料流
  orders = computed(() => this.OrderResource.value()?.items || []);
  totalPages = computed(() => this.OrderResource.value()?.totalPages|| 0);

  // 觸發搜尋的方法
  onSearch(){
    if (this.searchForm.invalid) {
     
      return;} 
  
    const formValue = this.searchForm.value;
    this.searchFilters.set({
      orderId: formValue.orderId || undefined,
      userName: formValue.userName || undefined,   // 這裡保留 userName
      startDate: formValue.startDate ? new Date(formValue.startDate) : undefined,
      endDate: formValue.endDate ? new Date(formValue.endDate) : undefined,
    });
  
    this.navigatePage(1);
  }

  onReset(): void {
    this.searchForm.reset();
    this.searchFilters.set({});
    this.navigatePage(1);
  }
  
  navigatePage(pageNumber: number): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page: pageNumber },
      queryParamsHandling: 'merge' 
    });
  }
  

  nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.navigatePage(this.currentPage() + 1);
    }
  }

  prevPage(): void {
    if (this.currentPage() > 1) {
       this.navigatePage(this.currentPage() - 1);
    }
  }

  get maxTodayString(): string {
    const today = new Date();
    return today.toISOString().split('T')[0]; // 回傳格式如 "2026-06-16"
  }

  updateOrderStatus(order: any): void {
    this.editingOrder = order;
  }

  handleStatusSave(newStatus: OrderStatus): void{
    if (!this.editingOrder) return;

    this.isModalLoading = true;

    // 注意：這裡假設你的訂單唯一識別碼是 snowflakeId
    const orderId = this.editingOrder.snowflakeId;
  }










  private dateRangeValidator(): ValidatorFn {
    return (group: AbstractControl): { [key: string]: any } | null => {
      const form = group as FormGroup;
      const start = form.get('startDate')?.value;
      const end = form.get('endDate')?.value;

      if (!start && !end) return null;

      

      const today = new Date();
      today.setHours(23, 59, 59, 999);

      let hasError = false;

      // 結束日期不能超過今天
      if (end) {
        const endDate = new Date(end);
        if (endDate > today) {
          form.get('endDate')?.setErrors({ futureDate: true });
          hasError = true;
        } else {
          form.get('endDate')?.setErrors(null);
        }
      }

      // 開始日期不能晚於結束日期
      if (start && end) {
        const startDate = new Date(start);
        const endDate = new Date(end);
        if (startDate > endDate) {
          form.get('startDate')?.setErrors({ dateRangeInvalid: true });
          hasError = true;
        } else {
          form.get('startDate')?.setErrors(null);
        }
      }
      // 若檢查通過，清除可能遺留的錯誤
      return hasError ? { dateRangeInvalid: true } : null;
    };
  }

  

  











  

  /*
  handleStatusSave(newStatus: OrderStatus): void {
    if (!this.editingOrder) return;
    
    this.isModalLoading = true;
    const orderId = this.editingOrder.id;

    // 發送給後端的 API（傳過去的會是 0, 1, 2... 等數字）
    this.orderApi.updateStatus(orderId, newStatus).subscribe({
      next: () => {
        const index = this.orders.findIndex(o => o.id === orderId);
        if (index !== -1) {
          // 更新本地主表格資料的狀態
          this.orders[index].status = newStatus;
          this.orders = [...this.orders]; 
        }
        this.editingOrder = null;
      },
      error: () => alert('更新失敗'),
      complete: () => this.isModalLoading = false
    });
  }
   */ 



  viewOrderDetail(order: any) {
    alert(`查看訂單 #${order.id} 詳細資訊 (功能開發中)`);
  }

  getStatusClass(status: OrderStatus): string {
      switch (status) {
        case OrderStatus.Completed:
          return 'bg-green-100 text-green-800'; // 綠色
          
        case OrderStatus.Cancelled:
          return 'bg-red-100 text-red-800'; // 紅色
          
        case OrderStatus.Pending:
          return 'bg-amber-100 text-amber-800'; // 琥珀色/深黃（待付款）
          
        case OrderStatus.Paid:
        case OrderStatus.Processing:
        case OrderStatus.Shipped:
          return 'bg-blue-100 text-blue-800'; // 藍色（處理中系列）
          
        default:
          return 'bg-gray-100 text-gray-800'; // 灰色
      }
    }
  
}
