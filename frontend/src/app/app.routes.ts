import { isDevMode } from '@angular/core';
import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/auth/auth.guards';
import { AdminLayout } from './layouts/admin-layout/admin-layout';
import { PublicLayout } from './layouts/public-layout/public-layout';

export const routes: Routes = [
  {
    path: '',
    component: PublicLayout,
    children: [
      { path: '', pathMatch: 'full', loadComponent: () => import('./features/public/home/home').then((m) => m.Home) },
      { path: 'login', loadComponent: () => import('./features/public/login/login').then((m) => m.Login) },
      // Component showcase, only reachable while developing
      ...(isDevMode()
        ? [
            { path: 'dev/components', loadComponent: () => import('./features/dev/ui-gallery/ui-gallery').then((m) => m.UiGallery) },
            {
              path: 'dev/components/display',
              loadComponent: () => import('./features/dev/ui-gallery-display/ui-gallery-display').then((m) => m.UiGalleryDisplay),
            },
          ]
        : []),
    ],
  },
  {
    path: 'admin',
    component: AdminLayout,
    canActivate: [authGuard, adminGuard],
    children: [
      { path: '', pathMatch: 'full', loadComponent: () => import('./features/admin/dashboard/dashboard').then((m) => m.Dashboard) },
      { path: 'news', loadComponent: () => import('./features/admin/news/news-list/news-list').then((m) => m.NewsList) },
      { path: 'news/new', loadComponent: () => import('./features/admin/news/news-edit/news-edit').then((m) => m.NewsEdit) },
      { path: 'news/:id', loadComponent: () => import('./features/admin/news/news-edit/news-edit').then((m) => m.NewsEdit) },
      { path: 'projects', loadComponent: () => import('./features/admin/projects/project-list/project-list').then((m) => m.ProjectList) },
      { path: 'projects/new', loadComponent: () => import('./features/admin/projects/project-edit/project-edit').then((m) => m.ProjectEdit) },
      { path: 'projects/:id', loadComponent: () => import('./features/admin/projects/project-edit/project-edit').then((m) => m.ProjectEdit) },
    ],
  },
  {
    path: '**',
    component: PublicLayout,
    children: [{ path: '', loadComponent: () => import('./features/public/not-found/not-found').then((m) => m.NotFound) }],
  },
];
