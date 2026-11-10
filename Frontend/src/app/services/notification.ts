import { inject, Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root',
})
export class Notification {
  private snackBar = inject(MatSnackBar);
  public showNotification(message: string) {
    this.snackBar.open(message, 'Close', { duration: 5000 });
  }
}
