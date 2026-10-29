import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { PagedResult } from '../../../../shared/data/types/paging.types';
import { HomeBanner, HomeNews, HomeProject } from '../types/home.types';

const SECTION_SIZE = 3;

@Injectable({ providedIn: 'root' })
export class HomeApi {
  private readonly http = inject(HttpClient);

  banners(): Observable<HomeBanner[]> {
    return this.http.get<HomeBanner[]>(`${API_BASE}/home-banners`);
  }

  latestNews(): Observable<HomeNews[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', SECTION_SIZE);
    return this.http.get<PagedResult<HomeNews>>(`${API_BASE}/news`, { params }).pipe(map((result) => result.items));
  }

  latestProjects(): Observable<HomeProject[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', SECTION_SIZE);
    return this.http.get<PagedResult<HomeProject>>(`${API_BASE}/projects`, { params }).pipe(map((result) => result.items));
  }
}
