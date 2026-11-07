import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { LecturerOption, MyProfile, MyStudentProject, SaveStudentProjectRequest } from '../types/student-project.types';

const PROJECTS_URL = `${API_BASE}/me/student-projects`;
const PROFILE_URL = `${API_BASE}/me/profile`;
const PICKER_PAGE_SIZE = 100;

@Injectable({ providedIn: 'root' })
export class StudentProjectsApi {
  private readonly http = inject(HttpClient);

  list(): Observable<MyStudentProject[]> {
    return this.http.get<MyStudentProject[]>(PROJECTS_URL);
  }

  get(id: string): Observable<MyStudentProject> {
    return this.http.get<MyStudentProject>(`${PROJECTS_URL}/${id}`);
  }

  create(request: SaveStudentProjectRequest): Observable<MyStudentProject> {
    return this.http.post<MyStudentProject>(PROJECTS_URL, request);
  }

  update(id: string, request: SaveStudentProjectRequest): Observable<MyStudentProject> {
    return this.http.put<MyStudentProject>(`${PROJECTS_URL}/${id}`, request);
  }

  setPublished(id: string, isPublished: boolean): Observable<MyStudentProject> {
    return this.http.patch<MyStudentProject>(`${PROJECTS_URL}/${id}/published`, { isPublished });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${PROJECTS_URL}/${id}`);
  }

  profile(): Observable<MyProfile> {
    return this.http.get<MyProfile>(PROFILE_URL);
  }

  setShowProfile(profile: MyProfile, showProfile: boolean): Observable<MyProfile> {
    return this.http.put<MyProfile>(PROFILE_URL, {
      fullName: profile.fullName,
      bio: profile.bio,
      imageFileId: profile.imageFileId,
      showEmail: profile.showEmail,
      showProfile,
      previousSchool: profile.student?.previousSchool ?? null,
      address: profile.student?.address ?? null,
    });
  }

  lecturers(): Observable<LecturerOption[]> {
    const params = new HttpParams().set('pageSize', PICKER_PAGE_SIZE);
    return this.http
      .get<PagedResult<{ id: string; prefix: string | null; fullName: string }>>(`${API_BASE}/lecturers`, { params })
      .pipe(map((result) => result.items.map((lecturer) => ({ id: lecturer.id, label: `${lecturer.prefix ?? ''}${lecturer.fullName}` }))));
  }
}
