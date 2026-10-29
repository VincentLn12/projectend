import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { AdminProject, AdminProjectListItem, PickerOption, SaveProjectRequest } from '../types/project.types';

const PROJECTS_URL = `${API_BASE}/admin/projects`;
const PICKER_PAGE_SIZE = 100; // the API's largest page

@Injectable({ providedIn: 'root' })
export class AdminProjectsApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery): Observable<PagedResult<AdminProjectListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    return this.http.get<PagedResult<AdminProjectListItem>>(PROJECTS_URL, { params });
  }

  get(id: string): Observable<AdminProject> {
    return this.http.get<AdminProject>(`${PROJECTS_URL}/${id}`);
  }

  create(request: SaveProjectRequest): Observable<AdminProject> {
    return this.http.post<AdminProject>(PROJECTS_URL, request);
  }

  update(id: string, request: SaveProjectRequest): Observable<AdminProject> {
    return this.http.put<AdminProject>(`${PROJECTS_URL}/${id}`, request);
  }

  setPublished(id: string, isPublished: boolean): Observable<AdminProject> {
    return this.http.patch<AdminProject>(`${PROJECTS_URL}/${id}/published`, { isPublished });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${PROJECTS_URL}/${id}`);
  }

  // People who can be the owner of a project.
  students(): Observable<PickerOption[]> {
    const params = new HttpParams().set('pageSize', PICKER_PAGE_SIZE);
    return this.http
      .get<PagedResult<{ id: string; fullName: string; studentCode: string | null }>>(`${API_BASE}/admin/users`, { params })
      .pipe(
        map((result) =>
          result.items.map((user) => ({
            id: user.id,
            label: user.studentCode ? `${user.studentCode} ${user.fullName}` : user.fullName,
          })),
        ),
      );
  }

  lecturers(): Observable<PickerOption[]> {
    const params = new HttpParams().set('pageSize', PICKER_PAGE_SIZE);
    return this.http
      .get<PagedResult<{ id: string; prefix: string | null; fullName: string }>>(`${API_BASE}/admin/lecturers`, { params })
      .pipe(map((result) => result.items.map((lecturer) => ({ id: lecturer.id, label: `${lecturer.prefix ?? ''}${lecturer.fullName}` }))));
  }
}
