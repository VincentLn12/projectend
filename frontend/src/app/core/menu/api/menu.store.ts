import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { API_BASE } from '../../api.config';
import { MENU_KIND_PAGE, MenuEntry } from '../types/menu.types';

// Shown only while the API cannot be reached, so the site never has an empty menu because of an outage.
const FALLBACK_PAGES: [string, string][] = [
  ['/', 'หน้าแรก'],
  ['/news', 'ข่าว'],
  ['/projects', 'โครงงาน'],
  ['/syllabus', 'หลักสูตร'],
  ['/courses', 'อบรมระยะสั้น'],
  ['/tech-subjects', 'เทคโนโลยี'],
  ['/lecturers', 'อาจารย์'],
];

export const FALLBACK_MENU: MenuEntry[] = FALLBACK_PAGES.map(([path, label]) => ({
  id: path,
  label,
  kind: MENU_KIND_PAGE,
  target: path,
  openInNewTab: false,
  children: [],
}));

@Injectable({ providedIn: 'root' })
export class MenuStore {
  private readonly http = inject(HttpClient);

  private readonly current = signal<MenuEntry[]>(FALLBACK_MENU);
  private requested = false;

  readonly menu = this.current.asReadonly();

  // Safe to call from several places: the menu is fetched once per page load.
  load(): void {
    if (this.requested) {
      return;
    }
    this.requested = true;

    this.http.get<MenuEntry[]>(`${API_BASE}/menu`).subscribe({
      next: (menu) => this.current.set(menu),
      error: () => undefined,
    });
  }
}
