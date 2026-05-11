import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
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
   loginData: LoginRequest ={email: '',password: ''}
   isLoading = this.authService.isLoading;

  async onLogin(){
    console.log('✅ Login button clicked!');
    
    if (!this.loginData.email || !this.loginData.password) {
      alert("請填寫 Email 和密碼");
      return;
    }
     this.isLoading.set(true);
    try {
      const success = await this.authService.login(this.loginData);
      if (success) {
        alert("🎉 登入成功！");
        this.router.navigate(['/']);
      }
    } catch (error: any) {
      alert(error.message || "登入失敗，請檢查帳號密碼");
    } finally {
       this.isLoading.set(false);
    }
  }


}
