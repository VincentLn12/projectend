import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { AdminJobHistory, PickerOption, SaveJobHistoryRequest } from '../types/job-history.types';

const URL = `${API_BASE}/admin/job-histories`;
const PICKER_PAGE_SIZE = 100; // the API's largest page

@Injectable({ providedIn: 'root' })
export class AdminJobHistoriesApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery): Observable<PagedResult<AdminJobHistory>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    return this.http.get<PagedResult<AdminJobHistory>>(URL, { params });
  }

  get(id: string): Observable<AdminJobHistory> {
    return this.http.get<AdminJobHistory>(`${URL}/${id}`);
  }

  create(request: SaveJobHistoryRequest): Observable<AdminJobHistory> {
    return this.http.post<AdminJobHistory>(URL, request);
  }

  update(id: string, request: SaveJobHistoryRequest): Observable<AdminJobHistory> {
    return this.http.put<AdminJobHistory>(`${URL}/${id}`, request);
  }

  setPublished(id: string, isPublished: boolean): Observable<AdminJobHistory> {
    return this.http.patch<AdminJobHistory>(`${URL}/${id}/published`, { isPublished });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${URL}/${id}`);
  }

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
}
