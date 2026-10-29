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
      { path: 'news', loadComponent: () => import('./features/public/news/news-list/news-list').then((m) => m.PublicNewsList) },
      { path: 'news/:id', loadComponent: () => import('./features/public/news/news-detail/news-detail').then((m) => m.PublicNewsDetail) },
      { path: 'projects', loadComponent: () => import('./features/public/projects/project-list/project-list').then((m) => m.PublicProjectList) },
      { path: 'projects/:id', loadComponent: () => import('./features/public/projects/project-detail/project-detail').then((m) => m.PublicProjectDetail) },
      { path: 'lecturers', loadComponent: () => import('./features/public/lecturers/lecturer-list/lecturer-list').then((m) => m.PublicLecturerList) },
      { path: 'syllabus', loadComponent: () => import('./features/public/syllabus/syllabus-page/syllabus-page').then((m) => m.PublicSyllabus) },
      { path: 'syllabus/:id', loadComponent: () => import('./features/public/syllabus/syllabus-page/syllabus-page').then((m) => m.PublicSyllabus) },
      { path: 'tech-subjects', loadComponent: () => import('./features/public/tech-subjects/tech-subject-list/tech-subject-list').then((m) => m.PublicTechSubjectList) },
      { path: 'tech-subjects/:id', loadComponent: () => import('./features/public/tech-subjects/tech-subject-detail/tech-subject-detail').then((m) => m.PublicTechSubjectDetail) },
      { path: 'courses', loadComponent: () => import('./features/public/courses/course-list/course-list').then((m) => m.PublicCourseList) },
      { path: 'courses/:id', loadComponent: () => import('./features/public/courses/course-detail/course-detail').then((m) => m.PublicCourseDetail) },
      {
        path: 'me/registrations',
        canActivate: [authGuard],
        loadComponent: () => import('./features/public/courses/my-registrations/my-registrations').then((m) => m.MyRegistrations),
      },
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
      { path: 'lecturers', loadComponent: () => import('./features/admin/lecturers/lecturer-list/lecturer-list').then((m) => m.LecturerList) },
      { path: 'lecturers/new', loadComponent: () => import('./features/admin/lecturers/lecturer-edit/lecturer-edit').then((m) => m.LecturerEdit) },
      { path: 'lecturers/:id', loadComponent: () => import('./features/admin/lecturers/lecturer-edit/lecturer-edit').then((m) => m.LecturerEdit) },
    ],
  },
  {
    path: '**',
    component: PublicLayout,
    children: [{ path: '', loadComponent: () => import('./features/public/not-found/not-found').then((m) => m.NotFound) }],
  },
];
