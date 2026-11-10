import { Component } from '@angular/core';
import { LucideUser } from '@lucide/angular';
import { Logout } from '../logout/logout.component';

@Component({
  imports: [LucideUser, Logout],
  selector: 'app-body-title',
  styleUrl: './body-title.component.css',
  templateUrl: './body-title.component.html',
})
export class BodyTitle {}
