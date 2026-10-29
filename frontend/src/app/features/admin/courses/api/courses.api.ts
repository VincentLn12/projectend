import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import {
  AdminCourse,
  AdminCourseListItem,
  Generation,
  PickerOption,
  RegistrationItem,
  SaveCourseRequest,
  SaveGenerationRequest,
} from '../types/course.types';

const ADMIN_URL = `${API_BASE}/admin`;
const COURSES_URL = `${ADMIN_URL}/courses`;
const PICKER_PAGE_SIZE = 100; // the API's largest page

@Injectable({ providedIn: 'root' })
export class AdminCoursesApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery): Observable<PagedResult<AdminCourseListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    return this.http.get<PagedResult<AdminCourseListItem>>(COURSES_URL, { params });
  }

  get(id: string): Observable<AdminCourse> {
    return this.http.get<AdminCourse>(`${COURSES_URL}/${id}`);
  }

  create(request: SaveCourseRequest): Observable<AdminCourse> {
    return this.http.post<AdminCourse>(COURSES_URL, request);
  }

  update(id: string, request: SaveCourseRequest): Observable<AdminCourse> {
    return this.http.put<AdminCourse>(`${COURSES_URL}/${id}`, request);
  }

  setPublished(id: string, isPublished: boolean): Observable<AdminCourse> {
    return this.http.patch<AdminCourse>(`${COURSES_URL}/${id}/published`, { isPublished });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${COURSES_URL}/${id}`);
  }

  createGeneration(courseId: string, request: SaveGenerationRequest): Observable<Generation> {
    return this.http.post<Generation>(`${COURSES_URL}/${courseId}/generations`, request);
  }

  updateGeneration(id: string, request: SaveGenerationRequest): Observable<Generation> {
    return this.http.put<Generation>(`${ADMIN_URL}/generations/${id}`, request);
  }

  deleteGeneration(id: string): Observable<void> {
    return this.http.delete<void>(`${ADMIN_URL}/generations/${id}`);
  }

  registrations(generationId: string): Observable<RegistrationItem[]> {
    return this.http.get<RegistrationItem[]>(`${ADMIN_URL}/generations/${generationId}/registrations`);
  }

  cancelRegistration(generationId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${ADMIN_URL}/generations/${generationId}/registrations/${userId}`);
  }

  lecturers(): Observable<PickerOption[]> {
    const params = new HttpParams().set('pageSize', PICKER_PAGE_SIZE);
    return this.http
      .get<PagedResult<{ id: string; prefix: string | null; fullName: string }>>(`${ADMIN_URL}/lecturers`, { params })
      .pipe(map((result) => result.items.map((lecturer) => ({ id: lecturer.id, label: `${lecturer.prefix ?? ''}${lecturer.fullName}` }))));
  }
}
