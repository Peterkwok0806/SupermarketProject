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

  isLoading = this.authService.isLoading;

  registerData: RegisterRequest = { username: '', email: '', password: '' };

  confirmPassword: string = ''; // 新增確認密碼欄位

  async registerUser(){
    if (!this.registerData.username || !this.registerData.email || !this.registerData.password) {
      alert("Please fill in all fields");
      return;
    }

    if (this.registerData.password !== this.confirmPassword) {
      alert("Passwords do not match!");
      return;
    }

    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*])(?=.{6,})/;
    if (!passwordRegex.test(this.registerData.password)) {
      alert("Password must be at least 6 characters long and include at least one uppercase letter, one lowercase letter, and one special character (!@#$%^&*)");
      return;
    }

    if (this.registerData.password.length < 6) {
    alert("Password must be at least 6 characters");
    return;
    }
    try{
        await this.authService.registerUser(this.registerData);
    }catch (err) {
    console.error(err);
    }
  }
}   
