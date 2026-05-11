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

  async registerUser(){
    if (!this.registerData.username || !this.registerData.email || !this.registerData.password) {
      alert("Please fill in all fields");
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
