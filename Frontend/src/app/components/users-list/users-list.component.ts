import { Component, inject, signal } from '@angular/core';
import { UserService } from '../../services/user.service';
import { MatTableModule } from '@angular/material/table';
import { LucidePen, LucideTrash } from '@lucide/angular';
import { Router } from '@angular/router';
import { Header } from '../header/header.component';
import { AsyncPipe } from '@angular/common';

@Component({
  imports: [MatTableModule, LucidePen, LucideTrash, Header, AsyncPipe],
  selector: 'app-users-list',
  styleUrl: './users-list.component.css',
  templateUrl: './users-list.component.html',
})
export class UsersList {
  private userService = inject(UserService);
  private router = inject(Router);
  userList = signal<IUser[]>([]);

  displayedColumns = [
    'action',
    'email',
    'education',
    'job',
    'dateOfBirth',
    'nationalId',
    'lastName',
    'firstName',
    'row',
  ];
  // public users$ = this.userService.getUsers();

  ngOnInit() {
    this.readUsers();
  }
  public readUsers(): void {
    this.userService.getUsers().subscribe({
      next: (response) => this.userList.set(response),
      error: () => alert('Retrieving the list of users failed. Please try again.'),
    });
  }
  public editUser(id: number): void {
    this.router.navigate(['/createuser', id]);
  }
  public deleteUsers(id: number): void {
    if (confirm('Are you sure you want to delete the user?')) this.handleDelete(id);
  }
  public searchUsers(nationalId: string) {
    if (nationalId.trim()) this.handleSearch(nationalId);
    else this.readUsers();
  }
  private handleDelete(id: number) {
    this.userService.deleteUser(id).subscribe({
      next: () => {
        this.userList.update((users) => users.filter((user) => user.id != id));
        alert('User successfully deleted.');
      },
    });
  }
  private handleSearch(nationalId: string) {
    this.userService.getUserByNationalId(nationalId).subscribe({
      next: (response) => this.userList.set([response]),
      error: (error) => {
        if (error.status === 404) alert('No user with this national ID was found.');
        else alert('Error connecting to the server, please try again.');
      },
    });
  }
}
