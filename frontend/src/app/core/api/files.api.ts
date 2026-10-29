import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../api.config';
import { FileDto } from '../models/file.models';

@Injectable({ providedIn: 'root' })
export class FilesApi {
  private readonly http = inject(HttpClient);

  // Needs a signed-in user. Returns the file's id, which is what news, projects etc. store.
  upload(file: File): Observable<FileDto> {
    const body = new FormData();
    body.append('file', file, file.name);
    return this.http.post<FileDto>(`${API_BASE}/files`, body);
  }

  // Anyone can open a file by its id (the API serves them publicly).
  url(fileId: string): string {
    return `${API_BASE}/files/${fileId}`;
  }
}
