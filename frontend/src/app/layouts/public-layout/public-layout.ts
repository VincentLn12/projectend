import { NgTemplateOutlet } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { MenuStore } from '../../core/menu/api/menu.store';
import { MENU_KIND_EXTERNAL, MENU_KIND_GROUP } from '../../core/menu/types/menu.types';
import { SiteSettingsStore } from '../../core/settings/api/site-settings.store';
import { Button } from '../../shared/ui/button/button';

@Component({
  selector: 'app-public-layout',
  imports: [NgTemplateOutlet, RouterOutlet, RouterLink, RouterLinkActive, Button],
  templateUrl: './public-layout.html',
  host: { '(document:keydown.escape)': 'closeMobileMenu()' },
})
export class PublicLayout {
  protected readonly auth = inject(AuthService);
  protected readonly siteSettings = inject(SiteSettingsStore);
  protected readonly menuStore = inject(MenuStore);
  protected readonly isMobileMenuOpen = signal(false);

  protected readonly kindExternal = MENU_KIND_EXTERNAL;
  protected readonly kindGroup = MENU_KIND_GROUP;

  constructor() {
    this.siteSettings.load();
    this.menuStore.load();
  }

  protected toggleMobileMenu(): void {
    this.isMobileMenuOpen.update((open) => !open);
  }

  protected closeMobileMenu(): void {
    this.isMobileMenuOpen.set(false);
  }
}
