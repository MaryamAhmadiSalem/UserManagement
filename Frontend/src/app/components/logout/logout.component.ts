import { Component, inject } from '@angular/core';
import { UserService } from '../../services/user.service';
import { Location } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  imports: [],
  selector: 'app-logout',
  styleUrl: './logout.component.css',
  templateUrl: './logout.component.html',
})
export class Logout {
  private services = inject(UserService);
  private router = inject(Router);
  public logout() {
    this.router.navigate(['/register'])
    this.services.postToken().subscribe();
  }
}
