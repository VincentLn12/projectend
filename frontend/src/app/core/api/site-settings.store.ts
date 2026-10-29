import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { API_BASE } from '../api.config';
import { SystemSetting } from '../models/settings.models';

@Injectable({ providedIn: 'root' })
export class SiteSettingsStore {
  private readonly http = inject(HttpClient);

  private readonly current = signal<SystemSetting | null>(null);
  private readonly failed = signal(false);
  private requested = false;

  readonly settings = this.current.asReadonly();
  readonly loadFailed = this.failed.asReadonly();

  // Safe to call from several places: the settings are fetched once.
  load(): void {
    if (this.requested) {
      return;
    }
    this.requested = true;

    this.http.get<SystemSetting>(`${API_BASE}/settings`).subscribe({
      next: (settings) => this.current.set(settings),
      error: () => {
        this.requested = false;
        this.failed.set(true);
      },
    });
  }
}
