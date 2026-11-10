import { Component, signal } from '@angular/core';
import {
  LucideUserRound,
  LucideContactRound,
  LucideBriefcaseBusiness,
  LucideUsersRound,
  LucideCalendarDays,
  LucideMail,
  LucideInfo,
} from '@lucide/angular';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { inject } from '@angular/core';
import { UserService } from '../../services/user.service';
import { Poster } from '../poster/poster.component';
import { ActivatedRoute } from '@angular/router';
import { BodyTitle } from '../body-title/body-title.component';
import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { Notification } from '../../services/notification';

@Component({
  imports: [
    LucideUserRound,
    LucideContactRound,
    LucideBriefcaseBusiness,
    LucideUsersRound,
    LucideCalendarDays,
    LucideMail,
    LucideInfo,
    ReactiveFormsModule,
    Poster,
    BodyTitle,
  ],
  selector: 'app-user-create',
  styleUrl: './user-create.component.css',
  templateUrl: './user-create.component.html',
})
export class UserCreate {
  rightColumn = ['firstName', 'nationalId', 'job'];
  leftColumn = ['lastName', 'dateOfBirth'];

  dropDownItems = ['', 'Diploma', 'Associate', 'Bachelor', 'Master', 'Phd'];

  userForm: FormGroup = new FormGroup({
    firstName: new FormControl<string>('', [Validators.required, Validators.maxLength(50)]),
    lastName: new FormControl<string>('', [Validators.required, Validators.maxLength(50)]),
    nationalId: new FormControl<string>('', [
      Validators.required,
      Validators.pattern(/^[0-9]{10}$/),
    ]),
    dateOfBirth: new FormControl<string>('', [Validators.required]),
    job: new FormControl<string>('', [Validators.required, Validators.maxLength(100)]),
    education: new FormControl<string>('', [Validators.required]),
    email: new FormControl<string>('', [Validators.required, Validators.email]),
  });

  maxDate = '2005-12-31';
  id: number | null = null;
  isActive = signal<boolean>(true);
  private userService = inject(UserService);
  private activatedRoute = inject(ActivatedRoute);
  private notificationService = inject(Notification);

  ngOnInit() {
    this.handleGetUserById();
    // this.userForm.controls['firstName'].valueChanges.subscribe((a) => {
    //   console.log('firstName: ', a);
    // });
    // this.userForm.valueChanges.subscribe((a) => {
    //   console.log('formsValue: ', a);
    // });
  }
  public onSubmit(): void {
    if (this.userForm.invalid) {
      this.notificationService.showNotification('This information is not reliable.');
      return;
    }
    if (this.id === null) this.handleCreateUser();
    else this.handleUpdateUser();
  }

  private handleGetUserById() {
    const id = this.activatedRoute.snapshot.paramMap.get('id');
    if (id !== null) {
      const userId = Number(id);
      this.id = userId;
      this.userService.getUserById(userId).subscribe({
        next: (respons) => this.userForm.patchValue(respons),
        error: (err) => {
          if (err.status === 403)
            this.notificationService.showNotification(
              'You cannot view information that does not belong to you.',
            );
        },
      });
    }
  }
  private handleCreateUser() {
    this.isActive.set(false);
    this.userService
      .postUsers(this.userForm.value)
      .pipe(finalize(() => this.isActive.set(true)))
      .subscribe({
        next: () =>
          this.notificationService.showNotification(
            'Your information has been successfully registered.',
          ),
        error: (error) => this.handleErrorResult(error),
      });
  }
  private handleUpdateUser() {
    if (this.id === null) return;
    this.isActive.set(false);
    this.userService
      .putUsers(this.id, this.userForm.value)
      .pipe(finalize(() => this.isActive.set(true)))
      .subscribe({
        next: () =>
          this.notificationService.showNotification(
            'Your information has been successfully edited.',
          ),
        error: (error) => this.handleErrorResult(error),
      });
  }
  private handleErrorResult(err: HttpErrorResponse) {
    if (err.error.includes('NationalIdAndEmail'))
      this.notificationService.showNotification('The NationalID and Email cannot be duplicates.');
    else if (err.error.includes('Email'))
      this.notificationService.showNotification('The email cannot be duplicates.');
    else if (err.error.includes('NationalId'))
      this.notificationService.showNotification('The NationalID cannot be duplicates.');
    else if (err.error.includes('ExsistAccount'))
      this.notificationService.showNotification('A user account cannot have two owners.');
  }
}
