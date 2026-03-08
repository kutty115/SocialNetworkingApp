import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

// OPTIONAL: if you want admin protection
//import { AdminGuard } from './guards/admin.guard';

export const routes: Routes = [
  // ✅ Default
  { path: '', pathMatch: 'full', redirectTo: 'feed' },

  // ✅ Public routes
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./pages/register/register').then(m => m.RegisterComponent),
  },

  // ✅ Protected routes (need login)
  {
    path: 'feed',
    canActivate: [AuthGuard],
    loadComponent: () =>
      import('./pages/feed/feed').then(m => m.FeedComponent),
  },
  {
    path: 'profile',
    canActivate: [AuthGuard],
    loadComponent: () =>
      import('./pages/profile/profile').then(m => m.ProfileComponent),
  },
  {
    path: 'friends',
    canActivate: [AuthGuard],
    loadComponent: () =>
      import('./pages/friends/friends').then(m => m.FriendsComponent),
  },
  {
    path: 'chat',
    canActivate: [AuthGuard],
    loadComponent: () =>
      import('./pages/chat/chat').then(m => m.ChatComponent),
  },

  // ✅ Admin route (only admin)
  {
    path: 'admin',
    canActivate: [AuthGuard],
    loadComponent: () =>
      import('./pages/admin/admin').then(m => m.AdminComponent),
  },

  // ✅ 404 page
  {
    path: 'not-found',
    loadComponent: () =>
      import('./pages/not-found/not-found').then(m => m.NotFoundComponent),
  },

  // ✅ Wildcard (any unknown route)
  { path: '**', redirectTo: 'not-found' },
];
