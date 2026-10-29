import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { SyllabusDetail, SyllabusListItem } from '../types/syllabus.types';

@Injectable({ providedIn: 'root' })
export class SyllabusApi {
  private readonly http = inject(HttpClient);

  list(): Observable<SyllabusListItem[]> {
    return this.http.get<SyllabusListItem[]>(`${API_BASE}/syllabuses`);
  }

  current(): Observable<SyllabusDetail> {
    return this.http.get<SyllabusDetail>(`${API_BASE}/syllabuses/current`);
  }

  get(id: string): Observable<SyllabusDetail> {
    return this.http.get<SyllabusDetail>(`${API_BASE}/syllabuses/${id}`);
  }
}
