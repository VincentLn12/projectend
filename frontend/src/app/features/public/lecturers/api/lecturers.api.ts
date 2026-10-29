import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { Lecturer } from '../types/lecturer.types';

@Injectable({ providedIn: 'root' })
export class LecturersApi {
  private readonly http = inject(HttpClient);

  list(): Observable<Lecturer[]> {
    return this.http.get<Lecturer[]>(`${API_BASE}/lecturers`);
  }
}
