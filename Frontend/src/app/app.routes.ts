import { Routes } from '@angular/router';
import { MainLayout } from './components/main-layout/main-layout';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'register',
    pathMatch: 'full',
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./components/register/register').then((component) => component.Register),
  },
  {
    path: 'mainlayout',
    component: MainLayout,
    children: [
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
    ],
  },
];