import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { AdminNews, AdminNewsListItem, SaveNewsRequest } from '../types/news.types';

const NEWS_URL = `${API_BASE}/admin/news`;

@Injectable({ providedIn: 'root' })
export class AdminNewsApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery): Observable<PagedResult<AdminNewsListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    return this.http.get<PagedResult<AdminNewsListItem>>(NEWS_URL, { params });
  }

  get(id: string): Observable<AdminNews> {
    return this.http.get<AdminNews>(`${NEWS_URL}/${id}`);
  }

  create(request: SaveNewsRequest): Observable<AdminNews> {
    return this.http.post<AdminNews>(NEWS_URL, request);
  }

  update(id: string, request: SaveNewsRequest): Observable<AdminNews> {
    return this.http.put<AdminNews>(`${NEWS_URL}/${id}`, request);
  }

  setPublished(id: string, isPublished: boolean): Observable<AdminNews> {
    return this.http.patch<AdminNews>(`${NEWS_URL}/${id}/published`, { isPublished });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${NEWS_URL}/${id}`);
  }
}
