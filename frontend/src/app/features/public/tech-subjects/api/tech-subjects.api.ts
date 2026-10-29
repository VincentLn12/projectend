import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { TechSubjectDetail, TechSubjectListItem } from '../types/tech-subject.types';

@Injectable({ providedIn: 'root' })
export class TechSubjectsApi {
  private readonly http = inject(HttpClient);

  list(): Observable<TechSubjectListItem[]> {
    return this.http.get<TechSubjectListItem[]>(`${API_BASE}/tech-subjects`);
  }

  get(id: string): Observable<TechSubjectDetail> {
    return this.http.get<TechSubjectDetail>(`${API_BASE}/tech-subjects/${id}`);
  }
}
