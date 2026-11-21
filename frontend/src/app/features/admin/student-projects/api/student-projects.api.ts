import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { AdminStudentProjectListItem } from '../types/student-project.types';

const STUDENT_PROJECTS_URL = `${API_BASE}/admin/student-projects`;

@Injectable({ providedIn: 'root' })
export class AdminStudentProjectsApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery): Observable<PagedResult<AdminStudentProjectListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    return this.http.get<PagedResult<AdminStudentProjectListItem>>(STUDENT_PROJECTS_URL, { params });
  }

  setPublished(id: string, isPublished: boolean): Observable<void> {
    return this.http.patch<void>(`${STUDENT_PROJECTS_URL}/${id}/published`, { isPublished });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${STUDENT_PROJECTS_URL}/${id}`);
  }
}
