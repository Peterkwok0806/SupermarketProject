import { Component, Inject} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-custom-snackbar',
  imports: [MatIconModule,CommonModule],
  templateUrl: './custom-snackbar.component.html',
  styleUrl: './custom-snackbar.component.css'
})
export class CustomSnackbarComponent {
  constructor(
    @Inject(MAT_SNACK_BAR_DATA) public data: { message: string; type: 'success' | 'error' },
    public snackBarRef: MatSnackBarRef<CustomSnackbarComponent>
  ) {}

  close() {
    this.snackBarRef.dismiss();
  }
}
