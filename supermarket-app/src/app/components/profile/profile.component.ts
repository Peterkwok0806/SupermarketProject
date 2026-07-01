import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { updateProfileRequest } from '../../models/auth';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, ReactiveFormsModule, MatSnackBarModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  currentUser = this.authService.currentUser;
  isLoggedIn = this.authService.isLoggedIn;

  profileForm!: FormGroup;
  passwordForm!: FormGroup;

  ngOnInit() {
    if (!this.isLoggedIn()) {
      this.router.navigate(['/login']);
    }

    // Initialize profile form
    this.profileForm = this.fb.group({
      username: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]]
    });

    // Initialize password form with validators
    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, this.passwordStrengthValidator()]],
      confirmPassword: ['', [Validators.required]]
    }, {
      validators: [this.passwordMatchValidator()]
    });

    // Populate profile form with current user data
    const user = this.currentUser();
    if (user) {
      this.profileForm.patchValue({
        username: user.username,
        email: user.email
      });
    }
  }

  // Easy access to form controls in template
  get f() {
    return this.profileForm.controls;
  }

  get pf() {
    return this.passwordForm.controls;
  }

  get passwordChecklist() {
    const errors = this.pf['newPassword']?.errors;
    const value = this.pf['newPassword']?.value;

    // All requirements met
    if (!errors && value) {
      return { length: true, uppercase: true, lowercase: true, specialChar: true, allowedChars: true };
    }

    // Has passwordStrength errors — return the individual boolean statuses
    // No value entered yet — default to all false (allowedChars true)
    return errors?.['passwordStrength'] || { length: false, uppercase: false, lowercase: false, specialChar: false, allowedChars: true };
  }

  async updateProfile() {
    if (this.profileForm.invalid) {
      this.showSnackBar('⚠️ 請填寫有效的 Email 和 Username', 'error-snackbar');
      return;
    }
    try {
      const data: updateProfileRequest = {
        username: this.profileForm.value.username,
        email: this.profileForm.value.email
      };
      await this.authService.updateProfile(data);
      this.showSnackBar('🎉 個人資料更新成功！(Token 已同步刷新)', 'success-snackbar');
    } catch (error: any) {
      this.showSnackBar(`❌ 更新失敗：${error.message || '請稍後再試'}`, 'error-snackbar');
    }
  }

  async changePassword() {
    if (this.passwordForm.invalid) {
      this.showSnackBar('⚠️ 請填寫所有欄位並符合密碼要求', 'error-snackbar');
      return;
    }

    try {
      await this.authService.changePassword(this.passwordForm.value);
      this.showSnackBar('🎉 密碼修改成功！下次登入請使用新密碼', 'success-snackbar');
      this.passwordForm.reset();
    } catch (error: any) {
      this.showSnackBar(`❌ 更新失敗：${error.message || '請稍後再試'}`, 'error-snackbar');
    }
  }

  logout() {
    if (confirm('確定要登出嗎？')) {
      this.authService.logout();
    }
  }

  // ===== Validators (same as Register component) =====

  passwordStrengthValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
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

  passwordMatchValidator(passwordKey = 'newPassword', confirmPasswordKey = 'confirmPassword'): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const password = control.get(passwordKey);
      const confirmPassword = control.get(confirmPasswordKey);

      // If fields don't exist or confirm password hasn't been entered yet, don't return error
      if (!password || !confirmPassword || !confirmPassword.value) return null;

      const isMatch = password.value === confirmPassword.value;
      return isMatch ? null : { passwordMismatch: true };
    };
  }

  private showSnackBar(message: string, cssClass: 'success-snackbar' | 'error-snackbar') {
    this.snackBar.open(message, '關閉', {
      duration: 3500,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: [cssClass]
    });
  }
}
