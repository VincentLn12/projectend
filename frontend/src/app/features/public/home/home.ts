import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SiteSettingsStore } from '../../../core/settings/api/site-settings.store';
import { ThaiDatePipe } from '../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../shared/ui/button/button';
import { FileImage } from '../../../shared/ui/file-image/file-image';
import { HomeApi } from './api/home.api';
import { HomeBanner, HomeNews, HomeProject } from './types/home.types';

const BANNER_INTERVAL_MS = 6000;

@Component({
  selector: 'app-home',
  imports: [RouterLink, ThaiDatePipe, Button, FileImage],
  templateUrl: './home.html',
})
export class Home implements OnInit {
  private readonly api = inject(HomeApi);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly siteSettings = inject(SiteSettingsStore);

  protected readonly banners = signal<HomeBanner[]>([]);
  protected readonly bannerIndex = signal(0);
  protected readonly news = signal<HomeNews[]>([]);
  protected readonly projects = signal<HomeProject[]>([]);

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
}
