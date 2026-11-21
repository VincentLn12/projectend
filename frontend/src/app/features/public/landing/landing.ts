import { Button } from '../../../shared/ui/button/button';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SiteSettingsStore } from '../../../core/settings/api/site-settings.store';
import { FileImage } from '../../../shared/ui/file-image/file-image';
import { HomeApi } from '../home/api/home.api';
import { HomeBanner } from '../home/types/home.types';

const BANNER_INTERVAL_MS = 5000;
const SWIPE_THRESHOLD_PX = 40;

@Component({
  selector: 'app-landing',
  imports: [Button, RouterLink, FileImage],
  templateUrl: './landing.html',
})
export class Landing implements OnInit {
  private readonly api = inject(HomeApi);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly siteSettings = inject(SiteSettingsStore);

  protected readonly banners = signal<HomeBanner[]>([]);
  protected readonly index = signal(0);
  private paused = false;
  private touchStartX = 0;

  constructor() {
    this.siteSettings.load();
  }

  ngOnInit(): void {
    this.api.banners().subscribe({ next: (items) => this.banners.set(items), error: () => undefined });

    const timer = setInterval(() => {
      if (!this.paused) {
        this.next();
      }
    }, BANNER_INTERVAL_MS);
    this.destroyRef.onDestroy(() => clearInterval(timer));
  }

  protected next(): void {
    const count = this.banners().length;
    if (count > 1) {
      this.index.update((i) => (i + 1) % count);
    }
  }

  protected previous(): void {
    const count = this.banners().length;
    if (count > 1) {
      this.index.update((i) => (i - 1 + count) % count);
    }
  }

  protected goTo(i: number): void {
    this.index.set(i);
  }

  protected pause(value: boolean): void {
    this.paused = value;
  }

  protected onTouchStart(event: TouchEvent): void {
    this.touchStartX = event.changedTouches[0].clientX;
  }

  protected onTouchEnd(event: TouchEvent): void {
    const delta = event.changedTouches[0].clientX - this.touchStartX;
    if (Math.abs(delta) >= SWIPE_THRESHOLD_PX) {
      delta < 0 ? this.next() : this.previous();
    }
  }
}
