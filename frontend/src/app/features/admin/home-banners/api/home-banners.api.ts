import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { AdminHomeBanner, SaveHomeBannerRequest } from '../types/home-banner.types';

const URL = `${API_BASE}/admin/home-banners`;

@Injectable({ providedIn: 'root' })
export class AdminHomeBannersApi {
  private readonly http = inject(HttpClient);

  list(): Observable<AdminHomeBanner[]> {
    return this.http.get<AdminHomeBanner[]>(URL);
  }

  get(id: string): Observable<AdminHomeBanner> {
    return this.http.get<AdminHomeBanner>(`${URL}/${id}`);
  }

  create(request: SaveHomeBannerRequest): Observable<AdminHomeBanner> {
    return this.http.post<AdminHomeBanner>(URL, request);
  }

  update(id: string, request: SaveHomeBannerRequest): Observable<AdminHomeBanner> {
    return this.http.put<AdminHomeBanner>(`${URL}/${id}`, request);
  }

  setPublished(id: string, isPublished: boolean): Observable<AdminHomeBanner> {
    return this.http.patch<AdminHomeBanner>(`${URL}/${id}/published`, { isPublished });
  }

  move(id: string, direction: 'up' | 'down'): Observable<void> {
    return this.http.post<void>(`${URL}/${id}/move`, { direction });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${URL}/${id}`);
  }

}
