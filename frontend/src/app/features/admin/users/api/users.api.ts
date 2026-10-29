import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { AdminUser, AdminUserListItem, CreateUserRequest, UpdateUserRequest } from '../types/user.types';

const USERS_URL = `${API_BASE}/admin/users`;

@Injectable({ providedIn: 'root' })
export class AdminUsersApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery, userType: number | null): Observable<PagedResult<AdminUserListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    if (userType) {
      params = params.set('userType', userType);
    }
    return this.http.get<PagedResult<AdminUserListItem>>(USERS_URL, { params });
  }

  get(id: string): Observable<AdminUser> {
    return this.http.get<AdminUser>(`${USERS_URL}/${id}`);
  }

  create(request: CreateUserRequest): Observable<AdminUser> {
    return this.http.post<AdminUser>(USERS_URL, request);
  }

  update(id: string, request: UpdateUserRequest): Observable<AdminUser> {
    return this.http.put<AdminUser>(`${USERS_URL}/${id}`, request);
  }

  resetPassword(id: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${USERS_URL}/${id}/reset-password`, { newPassword });
  }

  setLocked(id: string, isLocked: boolean): Observable<void> {
    return this.http.patch<void>(`${USERS_URL}/${id}/locked`, { isLocked });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${USERS_URL}/${id}`);
  }
}

// The API explains user-management problems in English; admins get the Thai version.
export function describeUserError(error: unknown): string {
  const detail = error instanceof HttpErrorResponse && typeof error.error?.detail === 'string' ? error.error.detail : '';

  if (detail.includes('email is already used')) {
    return 'อีเมลนี้ถูกใช้แล้ว';
  }
  if (detail.includes('student code is already used')) {
    return 'รหัสนักศึกษานี้ถูกใช้แล้ว';
  }
  if (detail.includes('StudentCode and EntryYear')) {
    return 'นักศึกษาต้องระบุรหัสนักศึกษาและปีที่เข้าศึกษา';
  }
  if (detail.includes('lock your own')) {
    return 'ไม่สามารถล็อกบัญชีของตัวเองได้';
  }
  if (detail.includes('delete your own')) {
    return 'ไม่สามารถลบบัญชีของตัวเองได้';
  }
  if (detail.includes('last administrator')) {
    return 'ไม่สามารถถอดผู้ดูแลระบบคนสุดท้ายได้';
  }
  return describeHttpError(error);
}
