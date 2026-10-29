import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { SiteSettingsStore } from '../../core/settings/api/site-settings.store';
import { AuthService } from '../../core/auth/auth.service';
import { Button } from '../../shared/ui/button/button';

@Component({
  selector: 'app-public-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Button],
  templateUrl: './public-layout.html',
})
export class PublicLayout {
  protected readonly auth = inject(AuthService);
  protected readonly siteSettings = inject(SiteSettingsStore);

  constructor() {
    this.siteSettings.load();
  }
}
