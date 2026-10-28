import { Component } from '@angular/core';
import { LucideUsers, LucideUserPlus, LucideList } from '@lucide/angular';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  imports: [LucideUsers, LucideUserPlus, LucideList, RouterLink, RouterLinkActive],
  selector: 'app-sidebar',
  styleUrl: './sidebar.component.css',
  templateUrl: './sidebar.component.html',
})
export class Sidebar {}
