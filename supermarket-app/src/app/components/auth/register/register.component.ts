import { Component, inject ,OnInit,signal} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { RegisterRequest } from '../../../models/auth';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit
{
  
  public authService = inject(AuthService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  registerForm!: FormGroup;
  registerData: RegisterRequest = { username: '', email: '', password: '' };
  pwdCheckingResult: any = null;

  // UI 狀態控制
  showModal = false;
  isLoading = signal(false);
  errorMessage: string | null = null;



 


  ngOnInit(){
    this.registerForm = this.fb.group({
      username: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, this.passwordStrengthValidator()]],
      confirmPassword: ['', [Validators.required]]
    },
      {
        validators: [this.passwordMatchValidator()] 
      });
    this.registerForm.valueChanges.subscribe(() =>{
      this.errorMessage = null;
    });
  }

  get f() { 
    return this.registerForm.controls; 
  }
  
  get passwordChecklist() {
    const errors = this.f['password']?.errors;
    const value = this.f['password']?.value;
    
    // 狀況 A：密碼完全符合所有規定
    if (!errors && value) {
      return { length: true, uppercase: true, lowercase: true, specialChar: true, allowedChars: true };
    }
    
    // 狀況 B：有密碼強度錯誤，直接返回內部的各項 boolean 狀態
    // 狀況 C：尚未輸入任何值，預設給全 false（除了合法字元預設為 true）
    return errors?.['passwordStrength'] || { length: false, uppercase: false, lowercase: false, specialChar: false, allowedChars: true };
  }


  async onSubmit(){
    if (this.registerForm.invalid) {
      this.errorMessage = 'Please fulfill all requirements before submitting.';
      return;
    }
    this.isLoading.set(true);

    this.registerData = {
      username: this.registerForm.value.username,
      email: this.registerForm.value.email,
      password: this.registerForm.value.password
    };

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

  passwordStrengthValidator(): ValidatorFn{
    return (control: AbstractControl): ValidationErrors | null =>{
      const pwd = control.value || '';

        const status = {
        length: pwd.length >= 8,
        uppercase: /[A-Z]/.test(pwd),
        lowercase: /[a-z]/.test(pwd),
        specialChar: /[!@#$%^&*]/.test(pwd),
        allowedChars: /^[A-Za-z0-9!@#$%^&*]*$/.test(pwd)
      };
      const hasError = !status.length || !status.uppercase || !status.lowercase || !status.specialChar || !status.allowedChars;
      return hasError ? { passwordStrength: status } : null;
    };
  }

  passwordMatchValidator(passwordKey = 'password', confirmPasswordKey = 'confirmPassword'): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const password = control.get(passwordKey);
    const confirmPassword = control.get(confirmPasswordKey);
    
    // 如果欄位不存在，或確認密碼還沒有輸入值，先不回傳錯誤
    if (!password || !confirmPassword || !confirmPassword.value) return null;
    
    const isMatch = password.value === confirmPassword.value;
    return isMatch ? null : { passwordMismatch: true };
  };
}


}   
