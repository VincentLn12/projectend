import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { StudentProjectDetail, StudentProjectListItem } from '../types/student-project.types';

const STUDENT_PROJECTS_URL = `${API_BASE}/student-projects`;

@Injectable({ providedIn: 'root' })
export class PublicStudentProjectsApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery, academicYear: number | null, semester: number | null): Observable<PagedResult<StudentProjectListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    if (academicYear) {
      params = params.set('academicYear', academicYear);
    }
    if (semester) {
      params = params.set('semester', semester);
    }
    return this.http.get<PagedResult<StudentProjectListItem>>(STUDENT_PROJECTS_URL, { params });
  }

  get(id: string): Observable<StudentProjectDetail> {
    return this.http.get<StudentProjectDetail>(`${STUDENT_PROJECTS_URL}/${id}`);
  }
}
