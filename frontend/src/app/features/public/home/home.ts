import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { SiteSettingsStore } from '../../../core/settings/api/site-settings.store';
import { ThaiDatePipe } from '../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../shared/ui/button/button';
import { FileImage } from '../../../shared/ui/file-image/file-image';
import { HomeApi } from './api/home.api';
import { HomeBanner, HomeNews, HomeProject } from './types/home.types';

const BANNER_INTERVAL_MS = 6000;
const YOUTUBE_ID_PATTERN = /(?:youtube\.com\/(?:watch\?v=|embed\/|shorts\/)|youtu\.be\/)([\w-]{11})/;

@Component({
  selector: 'app-home',
  imports: [RouterLink, ThaiDatePipe, Button, FileImage],
  templateUrl: './home.html',
})
export class Home implements OnInit {
  private readonly api = inject(HomeApi);
  private readonly destroyRef = inject(DestroyRef);
  private readonly sanitizer = inject(DomSanitizer);
  protected readonly siteSettings = inject(SiteSettingsStore);

  protected readonly banners = signal<HomeBanner[]>([]);
  protected readonly bannerIndex = signal(0);
  protected readonly news = signal<HomeNews[]>([]);
  protected readonly projects = signal<HomeProject[]>([]);

  private readonly allQuickLinks = [
    {
      path: '/syllabus',
      label: 'หลักสูตร',
      tag: 'B.Sc.',
      description: 'โครงสร้างหลักสูตร หน่วยกิต และรายวิชาของหลักสูตรวิทยาศาสตรบัณฑิต',
      action: 'ดูหลักสูตร',
      icon: 'M12 6.042A8.967 8.967 0 006 3.75c-1.052 0-2.062.18-3 .512v14.25A8.987 8.987 0 016 18c2.305 0 4.408.867 6 2.292m0-14.25a8.966 8.966 0 016-2.292c1.052 0 2.062.18 3 .512v14.25A8.987 8.987 0 0018 18a8.967 8.967 0 00-6 2.292m0-14.25v14.25',
    },
    {
      path: '/tech-subjects',
      label: 'เราเรียนอะไร',
      tag: 'STACK',
      description: 'วิชาและเทคโนโลยีที่นักศึกษาได้เรียนและลงมือปฏิบัติจริง',
      action: 'ดูเทคโนโลยี',
      icon: 'M17.25 6.75L22.5 12l-5.25 5.25m-10.5 0L1.5 12l5.25-5.25m7.5-3l-4.5 16.5',
    },
    {
      path: '/projects',
      label: 'โครงงานวิจัย',
      tag: 'R&D',
      description: 'ผลงานโครงงานและงานวิจัยของนักศึกษาในสาขา',
      action: 'ดูโครงงาน',
      icon: 'M9.75 3.104v5.714a2.25 2.25 0 01-.659 1.591L5 14.5M9.75 3.104c-.251.023-.501.05-.75.082m.75-.082a24.301 24.301 0 014.5 0m0 0v5.714c0 .597.237 1.17.659 1.591L19.8 15.3M14.25 3.104c.251.023.501.05.75.082M19.8 15.3l-1.57.393A9.065 9.065 0 0112 15a9.065 9.065 0 00-6.23-.693L5 14.5m14.8.8l1.402 1.402c1.232 1.232.65 3.318-1.067 3.611A48.309 48.309 0 0112 21c-2.773 0-5.491-.235-8.135-.687-1.718-.293-2.3-2.379-1.067-3.61L5 14.5',
    },
    {
      path: '/lecturers',
      label: 'คณาจารย์',
      tag: 'FACULTY',
      description: 'รายชื่ออาจารย์ประจำสาขา ตำแหน่ง และความเชี่ยวชาญ',
      action: 'ดูรายชื่อ',
      icon: 'M15 19.128a9.38 9.38 0 002.625.372 9.337 9.337 0 004.121-.952 4.125 4.125 0 00-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 018.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0111.964-3.07M12 6.375a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zm8.25 2.25a2.625 2.625 0 11-5.25 0 2.625 2.625 0 015.25 0z',
    },
    {
      path: '/courses',
      label: 'หลักสูตรอบรม',
      tag: 'LIFELONG',
      description: 'หลักสูตรอบรมระยะสั้นสำหรับนักศึกษาและบุคคลทั่วไป',
      action: 'ดูหลักสูตรอบรม',
      icon: 'M4.26 10.147a60.436 60.436 0 00-.491 6.347A48.627 48.627 0 0112 20.904a48.627 48.627 0 018.232-4.41 60.46 60.46 0 00-.491-6.347m-15.482 0a50.57 50.57 0 00-2.658-.813A59.905 59.905 0 0112 3.493a59.902 59.902 0 0110.399 5.84c-.896.248-1.783.52-2.658.814m-15.482 0A50.697 50.697 0 0112 13.489a50.702 50.702 0 017.74-3.342',
    },
    {
      path: '/news',
      label: 'ข่าวสาร',
      tag: 'UPDATES',
      description: 'ข่าวประชาสัมพันธ์ กิจกรรม และประกาศของสาขา',
      action: 'ดูข่าวทั้งหมด',
      icon: 'M12 7.5h1.5m-1.5 3h1.5m-7.5 3h7.5m-7.5 3h7.5m3-9h3.375c.621 0 1.125.504 1.125 1.125V18a2.25 2.25 0 01-2.25 2.25M16.5 7.5V18a2.25 2.25 0 002.25 2.25M16.5 7.5V4.875c0-.621-.504-1.125-1.125-1.125H4.125C3.504 3.75 3 4.254 3 4.875V18a2.25 2.25 0 002.25 2.25h13.5M6 7.5h3v3H6v-3z',
    },
  ];

  protected readonly quickLinks = computed(() => {
    const savedItems = this.siteSettings.settings()?.quickMenuItems ?? [];
    const position = (path: string): number => {
      const index = savedItems.findIndex((item) => item.key === path.slice(1));
      return index === -1 ? savedItems.length : index;
    };
    return this.allQuickLinks
      .filter((link) => savedItems.find((item) => item.key === link.path.slice(1))?.enabled !== false)
      .sort((first, second) => position(first.path) - position(second.path));
  });

  // The settings field may hold several links separated by commas.
  protected readonly videos = computed<{ url: string; embedUrl: SafeResourceUrl | null }[]>(() =>
    (this.siteSettings.settings()?.videoUrl ?? '')
      .split(',')
      .map((url) => url.trim())
      .filter((url) => url.length > 0)
      .map((url) => {
        const videoId = url.match(YOUTUBE_ID_PATTERN)?.[1];
        return {
          url,
          embedUrl: videoId ? this.sanitizer.bypassSecurityTrustResourceUrl(`https://www.youtube-nocookie.com/embed/${videoId}`) : null,
        };
      }),
  );

  protected readonly mapEmbedUrl = computed<SafeResourceUrl | null>(() => {
    const location = this.siteSettings.settings()?.mapLocation?.trim();
    return location
      ? this.sanitizer.bypassSecurityTrustResourceUrl(`https://maps.google.com/maps?q=${encodeURIComponent(location)}&output=embed`)
      : null;
  });

  protected readonly mapLinkUrl = computed(() => {
    const location = this.siteSettings.settings()?.mapLocation?.trim();
    return location ? `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(location)}` : null;
  });

  constructor() {
    this.siteSettings.load();
  }

  ngOnInit(): void {
    // A section that fails to load is simply left out: the rest of the page still works.
    this.api.banners().subscribe({ next: (banners) => this.banners.set(banners), error: () => undefined });
    this.api.latestNews().subscribe({ next: (items) => this.news.set(items), error: () => undefined });
    this.api.latestProjects().subscribe({ next: (items) => this.projects.set(items), error: () => undefined });

    const timer = setInterval(() => this.showNextBanner(), BANNER_INTERVAL_MS);
    this.destroyRef.onDestroy(() => clearInterval(timer));
  }

  protected showNextBanner(): void {
    const count = this.banners().length;
    if (count > 1) {
      this.bannerIndex.update((index) => (index + 1) % count);
    }
  }

  protected showPreviousBanner(): void {
    const count = this.banners().length;
    if (count > 1) {
      this.bannerIndex.update((index) => (index - 1 + count) % count);
    }
  }

  protected showBanner(index: number): void {
    this.bannerIndex.set(index);
  }
}
