import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { StudentPortfolio } from '../types/student.types';

@Injectable({ providedIn: 'root' })
export class PublicStudentsApi {
  private readonly http = inject(HttpClient);

  get(id: string): Observable<StudentPortfolio> {
    return this.http.get<StudentPortfolio>(`${API_BASE}/students/${id}`);
  }
}
