import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-header',
  imports: [RouterLink],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  private cartService = inject(CartService);
  private authService = inject(AuthService);

  totalItems = this.cartService.totalItems;
  currentUser = this.authService.currentUser;
  isLoggedIn = this.authService.isLoggedIn;

  logout() {
    if (confirm('確定要登出嗎？')) {
      this.authService.logout();
    }
  }
}
