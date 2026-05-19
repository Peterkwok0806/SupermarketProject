import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {  Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { updateProfileRequest } from '../../models/auth';

@Component({
  selector: 'app-profile',
  imports: [CommonModule,FormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit{
  private authService = inject(AuthService);
  private router = inject(Router);
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
  }

  async updateProfile() {
    if (!this.editData.email || !this.editData.username) {
      alert("請填寫 Email 和 username");
      return;
    }
    try {
       await this.authService.updateProfile(this.editData);
        alert("🎉 update successful!");
        this.router.navigate(['']);
    } catch (error: any) {
      alert(error.message || "登入失敗，請檢查帳號密碼");
    } finally {

    }
    
  }

  async changePassword() {
    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      alert("New passwords do not match!");
      return;
    }

    if (this.passwordData.newPassword.length < 6) {
      alert("New password must be at least 6 characters");
      return;
    }

    // TODO: 呼叫後端 Change Password API
    alert("Password changed successfully! (Backend API not implemented yet)");
    
    this.passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };
  }

  logout() {
    if (confirm('確定要登出嗎？')) {
      this.authService.logout();
    }
  }
}
