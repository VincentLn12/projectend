import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { Button } from '../button/button';

// Page numbers to show: always the first, the last and the neighbours of the current page; null stands for "…".
export function buildPageList(current: number, totalPages: number): (number | null)[] {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_unused, index) => index + 1);
  }

  const wanted = new Set([1, totalPages, current - 1, current, current + 1].filter((n) => n >= 1 && n <= totalPages));
  const sorted = Array.from(wanted).sort((a, b) => a - b);

  const result: (number | null)[] = [];
  sorted.forEach((page, index) => {
    if (index > 0 && page - sorted[index - 1] > 1) {
      result.push(null);
    }
    result.push(page);
  });
  return result;
}

// Page numbers start at 1, like the API.
@Component({
  selector: 'app-pagination',
  imports: [Button],
  templateUrl: './pagination.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Pagination {
  readonly page = input(1);
  readonly pageSize = input(10);
  readonly total = input(0);
  readonly pageChange = output<number>();

  protected readonly totalPages = computed(() => Math.max(1, Math.ceil(this.total() / this.pageSize())));
  protected readonly pages = computed(() => buildPageList(this.page(), this.totalPages()));
  protected readonly summary = computed(() => {
    if (this.total() === 0) {
      return 'ไม่มีรายการ';
    }
    const from = (this.page() - 1) * this.pageSize() + 1;
    const to = Math.min(this.page() * this.pageSize(), this.total());
    return `แสดง ${from}–${to} จาก ${this.total()} รายการ`;
  });

  protected go(page: number): void {
    if (page >= 1 && page <= this.totalPages() && page !== this.page()) {
      this.pageChange.emit(page);
    }
  }
}
