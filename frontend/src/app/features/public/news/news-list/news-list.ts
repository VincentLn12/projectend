import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { createPagedList } from '../../../../shared/data/paged-list';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { NewsApi } from '../api/news.api';
import { NewsListItem } from '../types/news.types';

@Component({
  selector: 'app-public-news-list',
  imports: [RouterLink, ThaiDatePipe, DataState, FileImage, Pagination, SearchBox],
  templateUrl: './news-list.html',
})
export class PublicNewsList implements OnInit {
  private readonly api = inject(NewsApi);

  protected readonly list = createPagedList<NewsListItem>((query) => this.api.list(query), 9);

  ngOnInit(): void {
    this.list.load();
  }
}
