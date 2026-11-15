import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from '../sidebar/sidebar.component';
import { UserService } from '../../services/user.service';

@Component({
  imports: [RouterOutlet, Sidebar],
  selector: 'app-main-layout',
  styleUrl: './main-layout.css',
  templateUrl: './main-layout.html',
})
export class MainLayout {
  private service = inject(UserService);
  ngOnInit() {
    this.service.checkTokenExpiration();
  }
}
