import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../../../../core/api.config';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { AdminMenuItem, MenuPageOption, SaveMenuItemRequest } from '../types/menu.types';

const MENU_URL = `${API_BASE}/admin/menu`;

@Injectable({ providedIn: 'root' })
export class AdminMenuApi {
  private readonly http = inject(HttpClient);

  list(): Observable<AdminMenuItem[]> {
    return this.http.get<AdminMenuItem[]>(MENU_URL);
  }

  pages(): Observable<MenuPageOption[]> {
    return this.http.get<MenuPageOption[]>(`${MENU_URL}/pages`);
  }

  get(id: string): Observable<AdminMenuItem> {
    return this.http.get<AdminMenuItem>(`${MENU_URL}/${id}`);
  }

  create(request: SaveMenuItemRequest): Observable<AdminMenuItem> {
    return this.http.post<AdminMenuItem>(MENU_URL, request);
  }

  update(id: string, request: SaveMenuItemRequest): Observable<AdminMenuItem> {
    return this.http.put<AdminMenuItem>(`${MENU_URL}/${id}`, request);
  }

  setVisible(id: string, isVisible: boolean): Observable<AdminMenuItem> {
    return this.http.patch<AdminMenuItem>(`${MENU_URL}/${id}/visible`, { isVisible });
  }

  reorder(parentId: string | null, orderedIds: string[]): Observable<void> {
    return this.http.put<void>(`${MENU_URL}/order`, { parentId, orderedIds });
  }

  reset(): Observable<void> {
    return this.http.post<void>(`${MENU_URL}/reset`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${MENU_URL}/${id}`);
  }
}

// The API explains menu problems in English; officers get the Thai version.
export function describeMenuError(error: unknown): string {
  const detail = error instanceof HttpErrorResponse && typeof error.error?.detail === 'string' ? error.error.detail : '';

  if (detail.includes('too many items')) {
    return 'เมนูมีรายการมากเกินไป กรุณาลบรายการที่ไม่ใช้ก่อน';
  }
  if (detail.includes('http or https')) {
    return 'ลิงก์ภายนอกต้องขึ้นต้นด้วย http:// หรือ https://';
  }
  if (detail.includes('one of the site pages')) {
    return 'กรุณาเลือกหน้าจากรายการ';
  }
  if (detail.includes('inside another group')) {
    return 'กลุ่มเมนูย่อยวางซ้อนในกลุ่มอื่นไม่ได้';
  }
  if (detail.includes('top-level menu group')) {
    return 'กรุณาเลือกกลุ่มเมนูที่อยู่ระดับบนสุด';
  }
  if (detail.includes('inside this group first')) {
    return 'กลุ่มนี้ยังมีเมนูย่อยอยู่ กรุณาย้ายหรือลบเมนูย่อยก่อนเปลี่ยนประเภท';
  }
  return describeHttpError(error);
}
