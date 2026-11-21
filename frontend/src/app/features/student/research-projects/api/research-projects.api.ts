import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { LecturerOption, MyResearchProject, MyResearchProjectListItem, SaveMyResearchProjectRequest } from '../types/research-project.types';

const PROJECTS_URL = `${API_BASE}/me/projects`;

@Injectable({ providedIn: 'root' })
export class MyResearchProjectsApi {
  private readonly http = inject(HttpClient);

  list(): Observable<MyResearchProjectListItem[]> {
    return this.http.get<MyResearchProjectListItem[]>(PROJECTS_URL);
  }

  get(id: string): Observable<MyResearchProject> {
    return this.http.get<MyResearchProject>(`${PROJECTS_URL}/${id}`);
  }

  create(request: SaveMyResearchProjectRequest): Observable<MyResearchProject> {
    return this.http.post<MyResearchProject>(PROJECTS_URL, request);
  }

  update(id: string, request: SaveMyResearchProjectRequest): Observable<MyResearchProject> {
    return this.http.put<MyResearchProject>(`${PROJECTS_URL}/${id}`, request);
  }

  setPublished(id: string, isPublished: boolean): Observable<MyResearchProject> {
    return this.http.patch<MyResearchProject>(`${PROJECTS_URL}/${id}/published`, { isPublished });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${PROJECTS_URL}/${id}`);
  }

  lecturers(): Observable<LecturerOption[]> {
    return this.http
      .get<{ id: string; prefix: string | null; fullName: string }[]>(`${API_BASE}/lecturers`)
      .pipe(map((lecturers) => lecturers.map((lecturer) => ({ id: lecturer.id, label: `${lecturer.prefix ?? ''}${lecturer.fullName}` }))));
  }
}
