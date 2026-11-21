import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import {
  AdminShortCourse,
  AdminShortCourseListItem,
  CertificateTemplateOption,
  SaveShortCourseRequest,
  ShortCourseRegistration,
} from '../types/short-course.types';

const SHORT_COURSES_URL = `${API_BASE}/admin/short-courses`;
const TEMPLATES_URL = `${API_BASE}/admin/certificate-templates`;

@Injectable({ providedIn: 'root' })
export class AdminShortCoursesApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery): Observable<PagedResult<AdminShortCourseListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    return this.http.get<PagedResult<AdminShortCourseListItem>>(SHORT_COURSES_URL, { params });
  }

  get(id: string): Observable<AdminShortCourse> {
    return this.http.get<AdminShortCourse>(`${SHORT_COURSES_URL}/${id}`);
  }

  create(request: SaveShortCourseRequest): Observable<AdminShortCourse> {
    return this.http.post<AdminShortCourse>(SHORT_COURSES_URL, request);
  }

  update(id: string, request: SaveShortCourseRequest): Observable<AdminShortCourse> {
    return this.http.put<AdminShortCourse>(`${SHORT_COURSES_URL}/${id}`, request);
  }

  setPublished(id: string, isPublished: boolean): Observable<AdminShortCourse> {
    return this.http.patch<AdminShortCourse>(`${SHORT_COURSES_URL}/${id}/published`, { isPublished });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${SHORT_COURSES_URL}/${id}`);
  }

  registrations(id: string): Observable<ShortCourseRegistration[]> {
    return this.http.get<ShortCourseRegistration[]>(`${SHORT_COURSES_URL}/${id}/registrations`);
  }

  cancelRegistration(id: string, registrationId: string): Observable<void> {
    return this.http.post<void>(`${SHORT_COURSES_URL}/${id}/registrations/${registrationId}/cancel`, {});
  }

  signInSheet(id: string): Observable<Blob> {
    return this.http.get(`${SHORT_COURSES_URL}/${id}/sign-in-sheet`, { responseType: 'blob' });
  }

  certificates(id: string): Observable<Blob> {
    return this.http.get(`${SHORT_COURSES_URL}/${id}/certificates`, { responseType: 'blob' });
  }

  certificate(id: string, registrationId: string): Observable<Blob> {
    return this.http.get(`${SHORT_COURSES_URL}/${id}/registrations/${registrationId}/certificate`, { responseType: 'blob' });
  }

  certificateTemplates(): Observable<CertificateTemplateOption[]> {
    return this.http.get<CertificateTemplateOption[]>(TEMPLATES_URL);
  }
}
