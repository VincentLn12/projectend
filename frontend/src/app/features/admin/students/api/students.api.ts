import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { AdminStudent, StudentCourseProject, StudentJobHistory, StudentProject } from '../types/student.types';

const JOB_HISTORY_PAGE_SIZE = 100;
const STUDENTS_URL = `${API_BASE}/admin/students`;

@Injectable({ providedIn: 'root' })
export class AdminStudentsApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery): Observable<PagedResult<AdminStudent>> {
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
    return this.http.get<PagedResult<AdminStudent>>(STUDENTS_URL, { params });
  }

  get(id: string): Observable<AdminStudent> {
    return this.http.get<AdminStudent>(`${STUDENTS_URL}/${id}`);
  }

  jobHistories(userId: string): Observable<StudentJobHistory[]> {
    const params = new HttpParams().set('userId', userId).set('page', 1).set('pageSize', JOB_HISTORY_PAGE_SIZE);
    return this.http
      .get<PagedResult<StudentJobHistory>>(`${API_BASE}/admin/job-histories`, { params })
      .pipe(map((result) => result.items));
  }

  projects(studentId: string): Observable<StudentProject[]> {
    const params = new HttpParams().set('studentId', studentId).set('page', 1).set('pageSize', JOB_HISTORY_PAGE_SIZE);
    return this.http
      .get<PagedResult<StudentProject>>(`${API_BASE}/admin/projects`, { params })
      .pipe(map((result) => result.items));
  }

  courseProjects(studentId: string): Observable<StudentCourseProject[]> {
    const params = new HttpParams().set('studentId', studentId).set('page', 1).set('pageSize', JOB_HISTORY_PAGE_SIZE);
    return this.http
      .get<PagedResult<StudentCourseProject>>(`${API_BASE}/admin/student-projects`, { params })
      .pipe(map((result) => result.items));
  }

  setCourseProjectPublished(id: string, isPublished: boolean): Observable<void> {
    return this.http.patch<void>(`${API_BASE}/admin/student-projects/${id}/published`, { isPublished });
  }

  deleteCourseProject(id: string): Observable<void> {
    return this.http.delete<void>(`${API_BASE}/admin/student-projects/${id}`);
  }

  entryYears(): Observable<number[]> {
    return this.http.get<number[]>(`${STUDENTS_URL}/entry-years`);
  }
}
