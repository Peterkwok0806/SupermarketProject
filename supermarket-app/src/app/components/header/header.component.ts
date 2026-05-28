import { Component, inject,signal,HostListener , computed} from '@angular/core';
import { RouterLink,Router } from '@angular/router';
import { CommonModule} from '@angular/common';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';
import { ProductService} from '../../services/product.service';
import { Subject} from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, tap } from 'rxjs/operators';
import { FormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';


@Component({
  selector: 'app-header',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  private cartService = inject(CartService);
  private authService = inject(AuthService);
  private productService = inject(ProductService);
  private router = inject(Router);

  // Signals
  isDropdownOpen = signal<boolean>(false);
  searchTerm = signal<string>('');

  totalItems = this.cartService.totalItems;
  currentUser = this.authService.currentUser;
  isLoggedIn = this.authService.isLoggedIn;

  // Search
  private searchTerms$ = new Subject<string>();
  showSuggestions = signal<boolean>(false);


  

  //用 toSignal 把 RxJS 資料流轉成唯讀 Signal
  // 預設值為空陣列 []
  suggestions = toSignal(
    this.searchTerms$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(term => {
        if (!term.trim()) this.showSuggestions.set(false);
      }),
      switchMap(term => this.productService.getSearchSuggestions(term)),
      tap(results => this.showSuggestions.set(results.length > 0))
    ),
    { initialValue: [] as string[] }
  );

  // Computed for template cleanliness
  hasSuggestions = computed(() => this.suggestions()?.length > 0);

  // 當使用者在 input 打字時觸發
  onSearchInput(term: string): void {
    this.searchTerm.set(term);
    this.searchTerms$.next(term);
  }

  // 點擊建議選單中的某一項
  selectSuggestion(term: string): void {
    this.searchTerm.set(term);
    this.showSuggestions.set(false);
    this.performSearch();
  }

  // 按下 Enter 或點擊建議後的搜尋跳轉
  onSearch() {
  this.performSearch();
  }

  private performSearch(): void {
    this.showSuggestions.set(false);
    const term = this.searchTerm().trim();
    
    this.router.navigate(['/products'], { 
      queryParams: { search: term || null } 
    });
  }

  // 當失去焦點時，延遲關閉選單（確保點擊事件能先被觸發）
  onBlur() {
    setTimeout(() => {
      this.showSuggestions.set(false);
    }, 150); // 微調為 150 毫秒
  }

  toggleDropdown(event: Event) {
    event.stopPropagation();
    this.isDropdownOpen.update(prev => !prev);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    this.isDropdownOpen.set(false);
  }

  onDropdownClick(event: Event): void {
    event.stopPropagation();
  }

  // Close suggestions when pressing Escape
  @HostListener('window:keydown.escape', ['$event'])
  onEscape(): void {
    this.showSuggestions.set(false);
    this.isDropdownOpen.set(false);
  }


  logout() {
    if (confirm('確定要登出嗎？')) {
      this.authService.logout();
    }
  }

  

}
