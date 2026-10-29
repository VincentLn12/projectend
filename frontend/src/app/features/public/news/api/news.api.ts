import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedQuery } from '../../../../shared/data/paged-list';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { NewsDetail, NewsListItem } from '../types/news.types';

@Injectable({ providedIn: 'root' })
export class NewsApi {
  private readonly http = inject(HttpClient);

  list(query: PagedQuery): Observable<PagedResult<NewsListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    return this.http.get<PagedResult<NewsListItem>>(`${API_BASE}/news`, { params });
  }

  get(id: string): Observable<NewsDetail> {
    return this.http.get<NewsDetail>(`${API_BASE}/news/${id}`);
  }
}
