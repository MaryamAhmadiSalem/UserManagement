import { Routes } from '@angular/router';
import { UserCreate } from './components/user-create/user-create.component';

export const routes: Routes = [
  {
    path: '',
    component: UserCreate,
  },
  {
    path: 'createuser',
    loadComponent: () =>
      import('./components/user-create/user-create.component').then(
        (component) => component.UserCreate,
      ),
  },
  {
    path: 'createuser/:id',
    loadComponent: () =>
      import('./components/user-create/user-create.component').then(
        (component) => component.UserCreate,
      ),
  },
  {
    path: 'users',
    loadComponent: () =>
      import('./components/users-list/users-list.component').then(
        (component) => component.UsersList,
      ),
  },
];
