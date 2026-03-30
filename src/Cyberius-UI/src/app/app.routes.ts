import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { adminGuard } from './core/guards/admin-guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'posts',
    loadComponent: () =>
      import('./features/posts/pages/post-list/post-list').then(
        (m) => m.PostList,
      ),
  },
  {
    path: 'posts/create',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/posts/pages/post-editor/post-editor').then(
        (m) => m.PostEditor,
      ),
  },
  {
    path: 'posts/:slug/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/posts/pages/post-editor/post-editor').then(
        (m) => m.PostEditor,
      ),
  },
  {
    path: 'posts/:slug',
    loadComponent: () =>
      import('./features/posts/pages/post-detail/post-detail').then(
        (m) => m.PostDetail,
      ),
  },
  {
    path: 'admin/categories',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./features/admin/category/category').then(
        (m) => m.Category,
      ),
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () => import('./features/profile/profile').then((m) => m.Profile),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
