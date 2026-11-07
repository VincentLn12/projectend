import { isDevMode } from '@angular/core';
import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/auth/auth.guards';
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
      { path: 'short-courses', loadComponent: () => import('./features/public/short-courses/short-course-list/short-course-list').then((m) => m.PublicShortCourseList) },
      { path: 'short-courses/my-registration', loadComponent: () => import('./features/public/short-courses/registration-lookup/registration-lookup').then((m) => m.RegistrationLookup) },
      { path: 'short-courses/:id', loadComponent: () => import('./features/public/short-courses/short-course-detail/short-course-detail').then((m) => m.PublicShortCourseDetail) },
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
    loadComponent: () => import('./layouts/admin-layout/admin-layout').then((m) => m.AdminLayout),
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
      { path: 'tech-subjects', loadComponent: () => import('./features/admin/tech-subjects/tech-subject-list/tech-subject-list').then((m) => m.TechSubjectList) },
      { path: 'tech-subjects/new', loadComponent: () => import('./features/admin/tech-subjects/tech-subject-edit/tech-subject-edit').then((m) => m.TechSubjectEdit) },
      { path: 'tech-subjects/:id', loadComponent: () => import('./features/admin/tech-subjects/tech-subject-edit/tech-subject-edit').then((m) => m.TechSubjectEdit) },
      { path: 'home-banners', loadComponent: () => import('./features/admin/home-banners/home-banner-list/home-banner-list').then((m) => m.HomeBannerList) },
      { path: 'home-banners/new', loadComponent: () => import('./features/admin/home-banners/home-banner-edit/home-banner-edit').then((m) => m.HomeBannerEdit) },
      { path: 'home-banners/:id', loadComponent: () => import('./features/admin/home-banners/home-banner-edit/home-banner-edit').then((m) => m.HomeBannerEdit) },
      { path: 'job-histories', loadComponent: () => import('./features/admin/job-histories/job-history-list/job-history-list').then((m) => m.JobHistoryList) },
      { path: 'job-histories/new', loadComponent: () => import('./features/admin/job-histories/job-history-edit/job-history-edit').then((m) => m.JobHistoryEdit) },
      { path: 'job-histories/:id', loadComponent: () => import('./features/admin/job-histories/job-history-edit/job-history-edit').then((m) => m.JobHistoryEdit) },
      { path: 'settings', loadComponent: () => import('./features/admin/settings/settings-page/settings-page').then((m) => m.SettingsPage) },
      { path: 'users', loadComponent: () => import('./features/admin/users/user-list/user-list').then((m) => m.UserList) },
      { path: 'users/new', loadComponent: () => import('./features/admin/users/user-edit/user-edit').then((m) => m.UserEdit) },
      { path: 'users/:id', loadComponent: () => import('./features/admin/users/user-edit/user-edit').then((m) => m.UserEdit) },
      { path: 'syllabuses', loadComponent: () => import('./features/admin/syllabuses/syllabus-list/syllabus-list').then((m) => m.SyllabusList) },
      { path: 'syllabuses/new', loadComponent: () => import('./features/admin/syllabuses/syllabus-edit/syllabus-edit').then((m) => m.SyllabusEdit) },
      { path: 'syllabuses/:id', loadComponent: () => import('./features/admin/syllabuses/syllabus-edit/syllabus-edit').then((m) => m.SyllabusEdit) },
      { path: 'short-courses', loadComponent: () => import('./features/admin/short-courses/short-course-list/short-course-list').then((m) => m.ShortCourseList) },
      { path: 'short-courses/new', loadComponent: () => import('./features/admin/short-courses/short-course-edit/short-course-edit').then((m) => m.ShortCourseEdit) },
      { path: 'short-courses/:id', loadComponent: () => import('./features/admin/short-courses/short-course-edit/short-course-edit').then((m) => m.ShortCourseEdit) },
      { path: 'short-courses/:id/registrations', loadComponent: () => import('./features/admin/short-courses/short-course-registrations/short-course-registrations').then((m) => m.ShortCourseRegistrations) },
      { path: 'certificate-templates', loadComponent: () => import('./features/admin/certificate-templates/certificate-template-list/certificate-template-list').then((m) => m.CertificateTemplateList) },
      { path: 'certificate-templates/new', loadComponent: () => import('./features/admin/certificate-templates/certificate-template-edit/certificate-template-edit').then((m) => m.CertificateTemplateEdit) },
      { path: 'certificate-templates/:id', loadComponent: () => import('./features/admin/certificate-templates/certificate-template-edit/certificate-template-edit').then((m) => m.CertificateTemplateEdit) },
    ],
  },
  {
    path: '**',
    component: PublicLayout,
    children: [{ path: '', loadComponent: () => import('./features/public/not-found/not-found').then((m) => m.NotFound) }],
  },
];
