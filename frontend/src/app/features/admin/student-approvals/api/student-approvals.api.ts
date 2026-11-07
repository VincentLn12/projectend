import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PendingStudent } from '../types/student-approval.types';

const APPROVALS_URL = `${API_BASE}/admin/student-approvals`;

@Injectable({ providedIn: 'root' })
export class AdminStudentApprovalsApi {
  private readonly http = inject(HttpClient);

  list(): Observable<PendingStudent[]> {
    return this.http.get<PendingStudent[]>(APPROVALS_URL);
  }

  approve(userId: string): Observable<void> {
    return this.http.post<void>(`${APPROVALS_URL}/${userId}/approve`, {});
  }

  reject(userId: string): Observable<void> {
    return this.http.delete<void>(`${APPROVALS_URL}/${userId}`);
  }
}
