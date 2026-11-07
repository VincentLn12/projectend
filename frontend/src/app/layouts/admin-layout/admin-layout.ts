import { NgTemplateOutlet } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { SiteSettingsStore } from '../../core/settings/api/site-settings.store';
import { Button } from '../../shared/ui/button/button';
import { FileImage } from '../../shared/ui/file-image/file-image';
import { MENU_ICONS } from './menu-icons';

export interface MenuItem {
  label: string;
  path: string;
  exact: boolean;
  icon: string;
}

export interface MenuGroup {
  groupName: string;
  items: MenuItem[];
}

@Component({
  selector: 'app-admin-layout',
  imports: [NgTemplateOutlet, RouterOutlet, RouterLink, RouterLinkActive, Button, FileImage],
  templateUrl: './admin-layout.html',
  host: { '(document:keydown.escape)': 'closeMobileMenu()' },
})
export class AdminLayout {
  protected readonly auth = inject(AuthService);
  protected readonly siteSettings = inject(SiteSettingsStore);
  protected readonly isMobileMenuOpen = signal(false);

  protected readonly userInitial = computed(() => {
    const name = this.auth.user()?.fullName?.trim();
    return name ? name.charAt(0).toUpperCase() : 'A';
  });

  protected readonly menuGroups: MenuGroup[] = [
    {
      groupName: 'ภาพรวม',
      items: [{ label: 'แดชบอร์ด', path: '/admin', exact: true, icon: 'dashboard' }],
    },
    {
      groupName: 'เนื้อหาและผลงาน',
      items: [
        { label: 'ข่าวสารและกิจกรรม', path: '/admin/news', exact: false, icon: 'news' },
        { label: 'โครงงานนักศึกษา', path: '/admin/projects', exact: false, icon: 'projects' },
        { label: 'แบนเนอร์ประชาสัมพันธ์', path: '/admin/home-banners', exact: false, icon: 'banners' },
      ],
    },
    {
      groupName: 'งานวิชาการและบุคลากร',
      items: [
        { label: 'ข้อมูลอาจารย์', path: '/admin/lecturers', exact: false, icon: 'lecturers' },
        { label: 'หลักสูตรการศึกษา', path: '/admin/syllabuses', exact: false, icon: 'syllabuses' },
        { label: 'อบรม', path: '/admin/short-courses', exact: false, icon: 'courses' },
        { label: 'รายวิชาเทคโนโลยี', path: '/admin/tech-subjects', exact: false, icon: 'tech-subjects' },
        { label: 'ประวัติการทำงานศิษย์เก่า', path: '/admin/job-histories', exact: false, icon: 'job-histories' },
      ],
    },
    {
      groupName: 'ผู้ใช้งานและการตั้งค่า',
      items: [
        { label: 'จัดการผู้ใช้งาน', path: '/admin/users', exact: false, icon: 'users' },
        { label: 'ตั้งค่าเว็บไซต์', path: '/admin/settings', exact: false, icon: 'settings' },
      ],
    },
  ];

  constructor() {
    this.siteSettings.load();
  }

  protected iconPaths(name: string): string[] {
    return MENU_ICONS[name] ?? [];
  }

  protected toggleMobileMenu(): void {
    this.isMobileMenuOpen.update((open) => !open);
  }

  protected closeMobileMenu(): void {
    this.isMobileMenuOpen.set(false);
  }
}
