import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { AdminSyllabus, AdminSyllabusListItem, SaveSyllabusRequest } from '../types/syllabus.types';

const SYLLABUSES_URL = `${API_BASE}/admin/syllabuses`;

@Injectable({ providedIn: 'root' })
export class AdminSyllabusesApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery): Observable<PagedResult<AdminSyllabusListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    if (query.sort) {
      params = params.set('sortBy', query.sort.key).set('sortDescending', query.sort.direction === 'desc');
    }
    for (const [name, value] of Object.entries(query.filters ?? {})) {
      params = params.set(name, value);
    }
    return this.http.get<PagedResult<AdminSyllabusListItem>>(SYLLABUSES_URL, { params });
  }

  get(id: string): Observable<AdminSyllabus> {
    return this.http.get<AdminSyllabus>(`${SYLLABUSES_URL}/${id}`);
  }

  create(request: SaveSyllabusRequest): Observable<AdminSyllabus> {
    return this.http.post<AdminSyllabus>(SYLLABUSES_URL, request);
  }

  update(id: string, request: SaveSyllabusRequest): Observable<AdminSyllabus> {
    return this.http.put<AdminSyllabus>(`${SYLLABUSES_URL}/${id}`, request);
  }

  setPublished(id: string, isPublished: boolean): Observable<AdminSyllabus> {
    return this.http.patch<AdminSyllabus>(`${SYLLABUSES_URL}/${id}/published`, { isPublished });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${SYLLABUSES_URL}/${id}`);
  }
}
