import { Component, output } from '@angular/core';
import { LucideSearch } from '@lucide/angular';
import { Highlight } from '../../directive/highlight.directive';

@Component({
  imports: [LucideSearch, Highlight],
  selector: 'app-header',
  styleUrl: './header.component.css',
  templateUrl: './header.component.html',
})
export class Header {
  searchEmitter = output<string>();

  public searchUsers(nationalId: string) {
    this.searchEmitter.emit(nationalId);
  }
  public onSearchInput(nationalId: string) {
    if (!nationalId.trim()) {
      this.searchEmitter.emit('');
    }
  }
}
