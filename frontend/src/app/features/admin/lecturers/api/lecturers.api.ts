import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { AdminLecturer, SaveLecturerRequest } from '../types/lecturer.types';

const LECTURERS_URL = `${API_BASE}/admin/lecturers`;

@Injectable({ providedIn: 'root' })
export class AdminLecturersApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery): Observable<PagedResult<AdminLecturer>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    return this.http.get<PagedResult<AdminLecturer>>(LECTURERS_URL, { params });
  }

  get(id: string): Observable<AdminLecturer> {
    return this.http.get<AdminLecturer>(`${LECTURERS_URL}/${id}`);
  }

  create(request: SaveLecturerRequest): Observable<AdminLecturer> {
    return this.http.post<AdminLecturer>(LECTURERS_URL, request);
  }

  update(id: string, request: SaveLecturerRequest): Observable<AdminLecturer> {
    return this.http.put<AdminLecturer>(`${LECTURERS_URL}/${id}`, request);
  }

  setPublished(id: string, isPublished: boolean): Observable<AdminLecturer> {
    return this.http.patch<AdminLecturer>(`${LECTURERS_URL}/${id}/published`, { isPublished });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${LECTURERS_URL}/${id}`);
  }
}
