import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { SiteSettingsStore } from '../../core/settings/api/site-settings.store';
import { AuthService } from '../../core/auth/auth.service';
import { Button } from '../../shared/ui/button/button';

const BUDDHIST_ERA_OFFSET = 543;

@Component({
  selector: 'app-public-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Button],
  templateUrl: './public-layout.html',
})
export class PublicLayout {
  protected readonly menu = [
    { path: '/home', label: 'หน้าหลัก', exact: true },
    { path: '/syllabus', label: 'หลักสูตรวิทย์คอม', exact: false },
    { path: '/projects', label: 'โครงการงานวิจัย', exact: false },
    { path: '/student-projects', label: 'โครงงานนักศึกษา', exact: false },
    { path: '/short-courses', label: 'หลักสูตรอบรม', exact: false },
    { path: '/lecturers', label: 'อาจารย์', exact: false },
  ];

  protected readonly auth = inject(AuthService);
  protected readonly siteSettings = inject(SiteSettingsStore);

  protected readonly menuOpen = signal(false);
  protected readonly memberMenuOpen = signal(false);
  protected readonly copyrightYear = new Date().getFullYear() + BUDDHIST_ERA_OFFSET;

  constructor() {
    this.siteSettings.load();
  }

  protected logout(): void {
    this.memberMenuOpen.set(false);
    this.menuOpen.set(false);
    this.auth.logout();
  }
}
