import { DestroyRef, inject, signal } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import { PagedResult } from './types/paging.types';
import { describeHttpError } from '../forms/server-errors';

export type SortDirection = 'asc' | 'desc';

export interface SortState {
  key: string;
  direction: SortDirection;
}

// sort and filters are only present when a page uses them, so a list without them sends the same query as before.
export interface PagedQuery {
  page: number;
  pageSize: number;
  search: string;
  sort?: SortState;
  filters?: Record<string, string>;
}

export type PagedFetcher<T> = (query: PagedQuery) => Observable<PagedResult<T>>;

// The state of a list page: the rows, the page, the search text, the sort order, the filters, and whether it is
// loading or failed. A page asks for new data with setPage / setSearch / setSort / setFilter / reload, and shows the
// signals with <app-data-table>, <app-pagination> and <app-search-box>. Only the newest request counts, so a slow
// answer to an old search can never overwrite a newer one.
export class PagedList<T> {
  readonly items = signal<T[]>([]);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly search = signal('');
  readonly sort = signal<SortState | null>(null);
  readonly filters = signal<Record<string, string>>({});
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

    this.subscription = this.fetcher(this.buildQuery()).subscribe({
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

  // Clicking a column: first click sorts ascending, the next one descending, and a different column starts ascending.
  toggleSort(key: string): void {
    const current = this.sort();
    const direction: SortDirection = current?.key === key && current.direction === 'asc' ? 'desc' : 'asc';
    this.sort.set({ key, direction });
    this.page.set(1);
    this.load();
  }

  // An empty value removes the filter. A changed filter starts from the first page.
  setFilter(name: string, value: string | null): void {
    const next = { ...this.filters() };
    if (value === null || value === '') {
      delete next[name];
    } else {
      next[name] = value;
    }

    if (JSON.stringify(next) === JSON.stringify(this.filters())) {
      return;
    }
    this.filters.set(next);
    this.page.set(1);
    this.load();
  }

  reload(): void {
    this.load();
  }

  destroy(): void {
    this.subscription?.unsubscribe();
  }

  private buildQuery(): PagedQuery {
    const query: PagedQuery = { page: this.page(), pageSize: this.pageSize, search: this.search() };
    const sort = this.sort();
    if (sort) {
      query.sort = sort;
    }
    if (Object.keys(this.filters()).length > 0) {
      query.filters = this.filters();
    }
    return query;
  }
}

// Creates a PagedList inside a component and stops its request when the component goes away.
export function createPagedList<T>(fetcher: PagedFetcher<T>, pageSize = 10): PagedList<T> {
  const list = new PagedList<T>(fetcher, pageSize);
  inject(DestroyRef).onDestroy(() => list.destroy());
  return list;
}
