import { Component, computed, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SiteSettingsStore } from '../../../../core/settings/api/site-settings.store';
import { createPagedList } from '../../../../shared/data/paged-list';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { NewsApi } from '../api/news.api';
import { NewsListItem } from '../types/news.types';

const PAGE_SIZE = 12;
const LATEST_COUNT = 5;

@Component({
  selector: 'app-public-news-list',
  imports: [RouterLink, ThaiDatePipe, DataState, FileImage, Pagination, SearchBox],
  templateUrl: './news-list.html',
})
export class PublicNewsList implements OnInit {
  private readonly api = inject(NewsApi);
  protected readonly siteSettings = inject(SiteSettingsStore);

  protected readonly list = createPagedList<NewsListItem>((query) => this.api.list(query), PAGE_SIZE);

  protected readonly quickLinks = [
    { label: 'หลักสูตร', path: '/syllabus' },
    { label: 'อาจารย์', path: '/lecturers' },
    { label: 'โครงงานวิจัยนักศึกษา', path: '/projects' },
  ];

  protected readonly featured = computed(() => {
    const showFeatured = this.list.page() === 1 && this.list.search() === '' && this.list.items().length > 1;
    return showFeatured ? this.list.items()[0] : null;
  });

  protected readonly restItems = computed(() => {
    const items = this.list.items();
    return this.featured() ? items.slice(1) : items;
  });

  protected readonly latest = computed(() => this.list.items().slice(0, LATEST_COUNT));

  protected initial(title: string): string {
    return (Array.from(title.trim())[0] ?? '?').toUpperCase();
  }

  ngOnInit(): void {
    this.siteSettings.load();
    this.list.load();
  }
}
