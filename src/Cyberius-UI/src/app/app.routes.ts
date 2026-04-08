import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { adminGuard } from './core/guards/admin-guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password').then((m) => m.ForgotPassword),
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./features/auth/reset-password/reset-password').then((m) => m.ResetPassword),
  },
  {
    path: 'confirm-email',
    loadComponent: () =>
      import('./features/auth/confirm-email-component/confirm-email-component').then(
        (m) => m.ConfirmEmailComponent,
      ),
  },
  {
    path: 'posts',
    loadComponent: () =>
      import('./features/posts/pages/post-list/post-list').then((m) => m.PostList),
  },
  {
    path: 'posts/create',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/posts/pages/post-editor/post-editor').then((m) => m.PostEditor),
  },
  {
    path: 'posts/drafts',
    canActivate: [authGuard],
    loadComponent: () => import('./features/posts/pages/drafts/drafts').then((m) => m.Drafts),
  },
  {
    path: 'posts/:slug/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/posts/pages/post-editor/post-editor').then((m) => m.PostEditor),
  },
  {
    path: 'posts/:slug',
    loadComponent: () =>
      import('./features/posts/pages/post-detail/post-detail').then((m) => m.PostDetail),
  },
  {
    path: 'users/:userId',
    loadComponent: () =>
      import('./features/users/pages/user-profile/user-profile').then((m) => m.UserProfileComponent),
  },
  {
    path: 'admin/categories',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/category/category').then((m) => m.Category),
  },
  {
    path: 'admin/users',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./features/admin/admin-user/admin-user').then((m) => m.AdminUserComponent),
  },
  {
    path: 'about',
    loadComponent: () => import('./features/about-me/about-me').then((m) => m.AboutMe),
  },
  {
    path: 'saved',
    loadComponent: () => import('./features/saved/saved').then((m) => m.Saved),
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () => import('./features/profile/profile').then((m) => m.Profile),
  },
  {
    path: '**',
    loadComponent: () => import('./features/not-found/not-found').then((m) => m.NotFound),
  },
];
