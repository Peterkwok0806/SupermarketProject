import { Injectable, inject } from '@angular/core';
import { MatSnackBar,} from '@angular/material/snack-bar';
import {CustomSnackbarComponent} from '../shared/custom-snackbar/custom-snackbar.component'

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

   private snackBar = inject(MatSnackBar); 

  

  success(message: string) {
    this.snackBar.openFromComponent(CustomSnackbarComponent, {
      data: { message, type: 'success' },
      duration: 4000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: ['success-snackbar']
    });
  }

  error(message: string) {
    this.snackBar.openFromComponent(CustomSnackbarComponent, {
      data: { message, type: 'error' },
      duration: 4000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: ['error-snackbar']
    });
  }
}
