import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { SiteSettings } from '../types/settings.types';

@Injectable({ providedIn: 'root' })
export class AdminSettingsApi {
  private readonly http = inject(HttpClient);

  // The public endpoint returns the same fields the admin form edits.
  get(): Observable<SiteSettings> {
    return this.http.get<SiteSettings>(`${API_BASE}/settings`);
  }

  save(settings: SiteSettings): Observable<SiteSettings> {
    return this.http.put<SiteSettings>(`${API_BASE}/admin/settings`, settings);
  }
}
