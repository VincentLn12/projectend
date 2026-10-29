import { DestroyRef, inject, signal } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import { PagedResult } from './types/paging.types';
import { describeHttpError } from '../forms/server-errors';

export interface PagedQuery {
  page: number;
  pageSize: number;
  search: string;
}

export type PagedFetcher<T> = (query: PagedQuery) => Observable<PagedResult<T>>;

// The state of a list page: the rows, the page, the search text, and whether it is loading or failed.
// A page asks for new data with setPage / setSearch / reload, and shows the signals with
// <app-data-table>, <app-pagination> and <app-search-box>. Only the newest request counts, so a slow
// answer to an old search can never overwrite a newer one.
export class PagedList<T> {
  readonly items = signal<T[]>([]);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly search = signal('');
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  private readonly fetcher: PagedFetcher<T>;
  readonly pageSize: number;
  private subscription: Subscription | null = null;

  constructor(fetcher: PagedFetcher<T>, pageSize = 10) {
    this.fetcher = fetcher;
    this.pageSize = pageSize;
  }

  load(): void {
    this.subscription?.unsubscribe();
    this.loading.set(true);
    this.error.set(null);

    this.subscription = this.fetcher({ page: this.page(), pageSize: this.pageSize, search: this.search() }).subscribe({
      next: (result) => {
        // The last row of the last page was deleted: step back to the new last page.
        if (result.items.length === 0 && result.total > 0 && this.page() > 1) {
          this.page.set(Math.ceil(result.total / this.pageSize));
          this.load();
          return;
        }
        this.items.set(result.items);
        this.total.set(result.total);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }

  setPage(page: number): void {
    this.page.set(page);
    this.load();
  }

  // A new search always starts from the first page.
  setSearch(text: string): void {
    if (text === this.search()) {
      return;
    }
    this.search.set(text);
    this.page.set(1);
    this.load();
  }

  reload(): void {
    this.load();
  }

  destroy(): void {
    this.subscription?.unsubscribe();
  }
}

// Creates a PagedList inside a component and stops its request when the component goes away.
export function createPagedList<T>(fetcher: PagedFetcher<T>, pageSize = 10): PagedList<T> {
  const list = new PagedList<T>(fetcher, pageSize);
  inject(DestroyRef).onDestroy(() => list.destroy());
  return list;
}
