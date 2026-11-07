import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../core/api.config';
import { MyJob, MyProfile, SaveMyJobRequest, SaveMyProfileRequest } from '../types/profile.types';

const PROFILE_URL = `${API_BASE}/me/profile`;
const JOBS_URL = `${API_BASE}/me/job-histories`;

@Injectable({ providedIn: 'root' })
export class ProfileApi {
  private readonly http = inject(HttpClient);

  get(): Observable<MyProfile> {
    return this.http.get<MyProfile>(PROFILE_URL);
  }

  save(request: SaveMyProfileRequest): Observable<MyProfile> {
    return this.http.put<MyProfile>(PROFILE_URL, request);
  }

  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${API_BASE}/me/change-password`, { currentPassword, newPassword });
  }

  jobs(): Observable<MyJob[]> {
    return this.http.get<MyJob[]>(JOBS_URL);
  }

  createJob(request: SaveMyJobRequest): Observable<MyJob> {
    return this.http.post<MyJob>(JOBS_URL, request);
  }

  updateJob(id: string, request: SaveMyJobRequest): Observable<MyJob> {
    return this.http.put<MyJob>(`${JOBS_URL}/${id}`, request);
  }

  deleteJob(id: string): Observable<void> {
    return this.http.delete<void>(`${JOBS_URL}/${id}`);
  }
}
