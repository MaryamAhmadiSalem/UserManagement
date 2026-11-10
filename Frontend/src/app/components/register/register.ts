import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { Notification } from '../../services/notification';

@Component({
  imports: [ReactiveFormsModule],
  selector: 'app-register',
  styleUrl: './register.css',
  templateUrl: './register.html',
})
export class Register {
  isFlipped: boolean = false;
  private router = inject(Router);
  private services = inject(UserService);
  private notificationService = inject(Notification);
  token: string = '';

  registrationLabels = [
    { label: 'email', type: 'email' },
    { label: 'password', type: 'password' },
    { label: 'confirmPassword', type: 'password' },
  ];
  loginLabels = [
    { label: 'email', type: 'email' },
    { label: 'password', type: 'password' },
  ];
  registerForm: FormGroup = new FormGroup(
    {
      email: new FormControl('', [Validators.required, Validators.email]),
      password: new FormControl('', [
        Validators.required,
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{10}$/),
      ]),
      confirmPassword: new FormControl('', [Validators.required]),
    },
    {
      validators: this.passwordMatchValidator.bind(this)
    },
  );
  loginForm: FormGroup = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [
      Validators.required,
      Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{10}$/),
    ]),
  });
  public onRegister(): void {
    if (this.registerForm.valid) {
      this.services.registerUser(this.registerForm.value).subscribe({
        next: (token) => {
          this.services.saveToken(token);
          this.router.navigate(['/mainlayout/createuser']);
        },
        error: (err) => {
          if (err.status === 401)
            this.notificationService.showNotification('This account exists in the system.');
        },
      });
    }
  }
  public onLogin(): void {
    if (this.loginForm.valid) {
      this.services.loginUser(this.loginForm.value).subscribe({
        next: (token) => {
          this.services.saveToken(token);
          this.router.navigate(['/mainlayout/users']);
        },
        error: (err) => {
          if (err.status === 401)
            this.notificationService.showNotification('The password is incorrect.');
          if (err.status === 404)
            this.notificationService.showNotification(
              'No account was found with this information.',
            );
        },
      });
    }
  }
  public passwordMatchValidator(registerForm : AbstractControl) {
    var valueForm = registerForm.value;
    if (valueForm.password !== valueForm.confirmPassword) return { passwordMismatch: true };
    return null;
  }
}
