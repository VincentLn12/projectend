import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import {
  LookupRequest,
  MyShortCourseRegistration,
  RegisterRequest,
  ShortCourseDetail,
  ShortCourseListItem,
  UpdateRegistrationRequest,
} from '../types/short-course.types';

const SHORT_COURSES_URL = `${API_BASE}/short-courses`;
const REGISTRATIONS_URL = `${API_BASE}/short-course-registrations`;

@Injectable({ providedIn: 'root' })
export class ShortCoursesApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery): Observable<PagedResult<ShortCourseListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    return this.http.get<PagedResult<ShortCourseListItem>>(SHORT_COURSES_URL, { params });
  }

  get(id: string): Observable<ShortCourseDetail> {
    return this.http.get<ShortCourseDetail>(`${SHORT_COURSES_URL}/${id}`);
  }

  register(id: string, request: RegisterRequest): Observable<MyShortCourseRegistration> {
    return this.http.post<MyShortCourseRegistration>(`${SHORT_COURSES_URL}/${id}/registrations`, request);
  }

  lookup(request: LookupRequest): Observable<MyShortCourseRegistration> {
    return this.http.post<MyShortCourseRegistration>(`${REGISTRATIONS_URL}/lookup`, request);
  }

  update(code: string, request: UpdateRegistrationRequest): Observable<MyShortCourseRegistration> {
    return this.http.put<MyShortCourseRegistration>(`${REGISTRATIONS_URL}/${encodeURIComponent(code)}`, request);
  }

  cancel(code: string, phone: string): Observable<void> {
    return this.http.post<void>(`${REGISTRATIONS_URL}/${encodeURIComponent(code)}/cancel`, { phone });
  }

  certificate(request: LookupRequest): Observable<Blob> {
    return this.http.post(`${REGISTRATIONS_URL}/certificate`, request, { responseType: 'blob' });
  }
}
