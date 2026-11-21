import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { AdminTechSubject, SaveTechSubjectRequest } from '../types/tech-subject.types';

const URL = `${API_BASE}/admin/tech-subjects`;

@Injectable({ providedIn: 'root' })
export class AdminTechSubjectsApi {
  private readonly http = inject(HttpClient);

  list(): Observable<AdminTechSubject[]> {
    return this.http.get<AdminTechSubject[]>(URL);
  }

  get(id: string): Observable<AdminTechSubject> {
    return this.http.get<AdminTechSubject>(`${URL}/${id}`);
  }

  create(request: SaveTechSubjectRequest): Observable<AdminTechSubject> {
    return this.http.post<AdminTechSubject>(URL, request);
  }

  update(id: string, request: SaveTechSubjectRequest): Observable<AdminTechSubject> {
    return this.http.put<AdminTechSubject>(`${URL}/${id}`, request);
  }

  setPublished(id: string, isPublished: boolean): Observable<AdminTechSubject> {
    return this.http.patch<AdminTechSubject>(`${URL}/${id}/published`, { isPublished });
  }

  move(id: string, direction: 'up' | 'down'): Observable<void> {
    return this.http.post<void>(`${URL}/${id}/move`, { direction });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${URL}/${id}`);
  }

}
