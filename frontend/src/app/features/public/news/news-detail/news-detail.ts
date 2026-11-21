import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { NewsApi } from '../api/news.api';
import { NewsDetail, NewsListItem } from '../types/news.types';

const OTHER_COUNT = 3;

@Component({
  selector: 'app-public-news-detail',
  imports: [RouterLink, ThaiDatePipe, DataState, FileImage],
  templateUrl: './news-detail.html',
})
export class PublicNewsDetail implements OnInit {
  private readonly api = inject(NewsApi);
  private readonly newsId = inject(ActivatedRoute).snapshot.paramMap.get('id')!;

  protected readonly news = signal<NewsDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly otherNews = signal<NewsListItem[]>([]);

  protected readonly galleryIds = computed(() => (this.news()?.imageFileIds ?? []).slice(1));

  ngOnInit(): void {
    this.load();
    this.api.list({ page: 1, pageSize: 6, search: '' }).subscribe({
      next: (result) => this.otherNews.set(result.items.filter((item) => item.id !== this.newsId).slice(0, OTHER_COUNT)),
    });
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get(this.newsId).subscribe({
      next: (news) => {
        this.news.set(news);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }
}
