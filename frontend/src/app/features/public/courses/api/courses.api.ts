import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { CourseDetail, CourseListItem, MyRegistration } from '../types/course.types';

@Injectable({ providedIn: 'root' })
export class CoursesApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery): Observable<PagedResult<CourseListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    return this.http.get<PagedResult<CourseListItem>>(`${API_BASE}/courses`, { params });
  }

  get(id: string): Observable<CourseDetail> {
    return this.http.get<CourseDetail>(`${API_BASE}/courses/${id}`);
  }

  register(generationId: string): Observable<MyRegistration> {
    return this.http.post<MyRegistration>(`${API_BASE}/generations/${generationId}/register`, {});
  }

  cancelRegistration(generationId: string): Observable<void> {
    return this.http.delete<void>(`${API_BASE}/generations/${generationId}/register`);
  }

  myRegistrations(): Observable<MyRegistration[]> {
    return this.http.get<MyRegistration[]>(`${API_BASE}/me/registrations`);
  }
}

// The API explains registration problems in English; visitors get the Thai version.
export function describeRegistrationError(error: unknown): string {
  const detail = error instanceof HttpErrorResponse && typeof error.error?.detail === 'string' ? error.error.detail : '';

  if (detail.includes('cancelled')) {
    return 'รุ่นนี้ถูกยกเลิกแล้ว';
  }
  if (detail.includes('closed')) {
    return 'ปิดรับสมัครแล้ว เพราะรุ่นนี้เริ่มอบรมไปแล้ว';
  }
  if (detail.includes('already registered')) {
    return 'คุณลงทะเบียนรุ่นนี้ไว้แล้ว';
  }
  if (detail.includes('full')) {
    return 'รุ่นนี้เต็มแล้ว';
  }
  return describeHttpError(error);
}
