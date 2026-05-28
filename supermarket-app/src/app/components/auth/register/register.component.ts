import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { RegisterRequest, AuthResponse } from '../../../models/auth';

@Component({
  selector: 'app-register',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  public authService = inject(AuthService);
  private router = inject(Router);

  showModal = false;

  isLoading = this.authService.isLoading;

  registerData: RegisterRequest = { username: '', email: '', password: '' };

  confirmPassword: string = ''; 

  errorMessage = '';

  passwordChecklist = {
    length: false,
    uppercase: false,
    lowercase: false,
    specialChar: false,
    allowedChars: true // 預設為 true，若輸入非法字元才變 false
  };

  onPasswordInput() {
    const pwd = this.registerData.password;

    if (pwd== null)
      return;

    // 1. 長度檢查 (至少 8 碼)
    this.passwordChecklist.length = pwd.length >= 8;

    // 2. 大寫字母檢查
    this.passwordChecklist.uppercase = /[A-Z]/.test(pwd);

    // 3. 小寫字母檢查
    this.passwordChecklist.lowercase = /[a-z]/.test(pwd);

    // 4. 特殊字元檢查
    this.passwordChecklist.specialChar = /[!@#$%^&*]/.test(pwd);

    // 5. 是否含有非法字元 (如果空字串算符合合法範圍)
    const allowedPasswordRegex = /^[A-Za-z0-9!@#$%^&*]*$/;
    this.passwordChecklist.allowedChars = allowedPasswordRegex.test(pwd);
  }

  async onSubmit(){
    this.errorMessage = '';

    if (!this.registerData.username || !this.registerData.email || !this.registerData.password) {
      this.errorMessage ="Please fill in all fields";
      return;
    }

    if (this.registerData.password !== this.confirmPassword) {
      this.errorMessage = 'password not match';
      return;
    }

    const allowedPasswordRegex = /^[A-Za-z0-9!@#$%^&*]{8,}$/;
    if (!allowedPasswordRegex.test(this.registerData.password)) {
      this.errorMessage = "Password can only contain English letters (A-Z, a-z), numbers (0-9), and special characters (!@#$%^&*)";
      return;
    }
    if (!/[A-Z]/.test(this.registerData.password)) {
      this.errorMessage = "Password must contain at least one uppercase letter (A-Z)";
      return;
    }

    if (!/[a-z]/.test(this.registerData.password)) {
      this.errorMessage = "Password must contain at least one lowercase letter (a-z)";
      return;
    }

    if (!/[!@#$%^&*]/.test(this.registerData.password)) {
      this.errorMessage = "Password must contain at least one special character (!@#$%^&*)";
      return;
    }

    try{
        await this.authService.registerUser(this.registerData);
         this.showModal = true;
    }catch (error:any) {
    this.errorMessage = error.message || 'Registration failed'; 
    }
  }

  closeModalAndNavigate() {
    this.showModal = false;
    this.router.navigate(['/login']);
  }



}   
