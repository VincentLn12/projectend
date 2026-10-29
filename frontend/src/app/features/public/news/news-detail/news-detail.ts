import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { NewsApi } from '../api/news.api';
import { NewsDetail } from '../types/news.types';

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

  ngOnInit(): void {
    this.load();
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
