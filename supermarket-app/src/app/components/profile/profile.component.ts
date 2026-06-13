import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {  Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { updateProfileRequest } from '../../models/auth';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-profile',
  imports: [CommonModule,FormsModule, MatSnackBarModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit{
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  currentUser = this.authService.currentUser;
  isLoggedIn = this.authService.isLoggedIn;


  editData: updateProfileRequest = {
    username: '',
    email: ''
  };

  passwordData = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

ngOnInit() {
    if (!this.isLoggedIn()) {
      // 如果未登入，跳轉到登入頁面
     this.router.navigate(['/login']);
    }

    const user = this.currentUser();
    if (user) {
      this.editData.username = user.username;
      this.editData.email = user.email;
    }
  }
  

  

  async updateProfile() {
    if (!this.editData.email || !this.editData.username) {
      this.showSnackBar('⚠️ 請填寫 Email 和 Username', 'error-snackbar');
      return;
      
    }
    try {
       await this.authService.updateProfile(this.editData);
        this.showSnackBar('🎉 個人資料更新成功！(Token 已同步刷新)', 'success-snackbar');
    } catch (error: any) {
      this.showSnackBar(`❌ 更新失敗：${error.message || '請稍後再試'}`, 'error-snackbar');
    } 
  }

  async changePassword() {
    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.showSnackBar('⚠️ 新密碼與確認密碼不一致！', 'error-snackbar');
      return;
    }

    if (this.passwordData.newPassword.length < 6) {
      this.showSnackBar('⚠️ 新密碼長度必須至少為 6 個字元', 'error-snackbar');
      return;
    }

    try {
       await this.authService.changePassword(this.passwordData);
        this.showSnackBar('🎉 密碼修改成功！下次登入請使用新密碼', 'success-snackbar');
    } catch (error: any) {
      this.showSnackBar(`❌ 更新失敗：${error.message || '請稍後再試'}`, 'error-snackbar');
    } 
    this.passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };
  }

  logout() {
    if (confirm('確定要登出嗎？')) {
      this.authService.logout();
    }
  }

  private showSnackBar(message: string, cssClass: 'success-snackbar' | 'error-snackbar') {
    this.snackBar.open(message, '關閉', {
      duration: 3500,           // 顯示 3.5 秒
      horizontalPosition: 'center', // 置中
      verticalPosition: 'bottom',   // 靠下
      panelClass: [cssClass]     // 帶入自訂樣式名稱
    });
  }


}
