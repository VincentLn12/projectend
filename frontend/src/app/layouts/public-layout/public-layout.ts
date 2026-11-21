import { NgTemplateOutlet } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MenuStore } from '../../core/menu/api/menu.store';
import { MENU_KIND_EXTERNAL, MENU_KIND_GROUP, MenuEntry } from '../../core/menu/types/menu.types';
import { SiteSettingsStore } from '../../core/settings/api/site-settings.store';
import { AuthService } from '../../core/auth/auth.service';
import { Button } from '../../shared/ui/button/button';

const BUDDHIST_ERA_OFFSET = 543;

@Component({
  selector: 'app-public-layout',
  imports: [NgTemplateOutlet, RouterOutlet, RouterLink, RouterLinkActive, Button],
  templateUrl: './public-layout.html',
})
export class PublicLayout {
  protected readonly menuStore = inject(MenuStore);
  protected readonly kindExternal = MENU_KIND_EXTERNAL;
  protected readonly kindGroup = MENU_KIND_GROUP;

  // The footer lists every link on one level: groups are replaced by their items.
  protected readonly footerLinks = computed<MenuEntry[]>(() =>
    this.menuStore.menu().flatMap((entry) => (entry.kind === MENU_KIND_GROUP ? entry.children : [entry])),
  );

  protected readonly auth = inject(AuthService);
  protected readonly siteSettings = inject(SiteSettingsStore);

  protected readonly menuOpen = signal(false);
  protected readonly memberMenuOpen = signal(false);
  protected readonly copyrightYear = new Date().getFullYear() + BUDDHIST_ERA_OFFSET;

  constructor() {
    this.siteSettings.load();
    this.menuStore.load();
  }

  protected logout(): void {
    this.memberMenuOpen.set(false);
    this.menuOpen.set(false);
    this.auth.logout();
  }
}
