import { Component, inject } from '@angular/core';
import { SiteSettingsStore } from '../../../core/settings/api/site-settings.store';

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
})
export class Home {
  protected readonly siteSettings = inject(SiteSettingsStore);

  constructor() {
    this.siteSettings.load();
  }
}
