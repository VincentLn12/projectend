import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { ProjectDetail, ProjectListItem } from '../types/project.types';

@Injectable({ providedIn: 'root' })
export class ProjectsApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery, academicYear: number | null): Observable<PagedResult<ProjectListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    if (academicYear) {
      params = params.set('academicYear', academicYear);
    }
    return this.http.get<PagedResult<ProjectListItem>>(`${API_BASE}/projects`, { params });
  }

  get(id: string): Observable<ProjectDetail> {
    return this.http.get<ProjectDetail>(`${API_BASE}/projects/${id}`);
  }
}
