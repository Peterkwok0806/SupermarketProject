import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { NotificationService } from '../../../services/notification.service';
import { LoginRequest } from '../../../models/auth';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
   private authService = inject(AuthService);
   private router = inject(Router);
   private notificationService = inject(NotificationService);
   loginData: LoginRequest = { email: '', password: '' };

  // 直接引用 AuthService 的 isLoading，不需本地管理
  isLoading = this.authService.isLoading;

  async onLogin() {
    if (!this.loginData.email || !this.loginData.password) {
      this.notificationService.error("請填寫 Email 和密碼");
      return;
    }

    // AuthService.login() 已經管理 isLoading 狀態 + 自動導頁 + 儲存 token
    // 此處只需呼叫並處理錯誤回饋
    try {
      const success = await this.authService.login(this.loginData);
      if (success) {
        this.notificationService.success("🎉 登入成功！");
        // 導頁已由 AuthService.login() 內部處理，無需重複呼叫
      }
    } catch (error: any) {
      this.notificationService.error(error.message || "登入失敗，請檢查帳號密碼");
    }
    // 不需要在 finally 中 set isLoading — AuthService.login() 已處理
  }
}
