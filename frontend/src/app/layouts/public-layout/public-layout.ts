import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { SiteSettingsStore } from '../../core/settings/api/site-settings.store';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-public-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './public-layout.html',
})
export class PublicLayout {
  protected readonly auth = inject(AuthService);
  protected readonly siteSettings = inject(SiteSettingsStore);

  constructor() {
    this.siteSettings.load();
  }
}
