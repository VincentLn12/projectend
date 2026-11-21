import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { CertificateTemplate, SaveCertificateTemplateRequest } from '../types/certificate-template.types';

const TEMPLATES_URL = `${API_BASE}/admin/certificate-templates`;

@Injectable({ providedIn: 'root' })
export class AdminCertificateTemplatesApi {
  private readonly http = inject(HttpClient);

  list(): Observable<CertificateTemplate[]> {
    return this.http.get<CertificateTemplate[]>(TEMPLATES_URL);
  }

  get(id: string): Observable<CertificateTemplate> {
    return this.http.get<CertificateTemplate>(`${TEMPLATES_URL}/${id}`);
  }

  create(request: SaveCertificateTemplateRequest): Observable<CertificateTemplate> {
    return this.http.post<CertificateTemplate>(TEMPLATES_URL, request);
  }

  update(id: string, request: SaveCertificateTemplateRequest): Observable<CertificateTemplate> {
    return this.http.put<CertificateTemplate>(`${TEMPLATES_URL}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${TEMPLATES_URL}/${id}`);
  }
}
