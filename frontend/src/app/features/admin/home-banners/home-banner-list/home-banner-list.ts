import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { PageHeader } from '../../../../shared/ui/page-header/page-header';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminHomeBannersApi } from '../api/home-banners.api';
import { AdminHomeBanner } from '../types/home-banner.types';

export type BannerStatusFilter = 'all' | 'published' | 'draft';

@Component({
  selector: 'app-home-banner-list',
  imports: [RouterLink, ReactiveFormsModule, ThaiDatePipe, Button, DataState, FileImage, PageHeader, SearchBox, SelectInput],
  templateUrl: './home-banner-list.html',
})
export class HomeBannerList implements OnInit {
  private readonly api = inject(AdminHomeBannersApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly rows = signal<AdminHomeBanner[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly statusFilter = signal<BannerStatusFilter>('all');

  protected readonly statusOptions: SelectOption<BannerStatusFilter>[] = [
    { value: 'published', label: 'เผยแพร่' },
    { value: 'draft', label: 'ฉบับร่าง' },
  ];
  protected readonly statusControl = new FormControl<BannerStatusFilter | null>(null);

  protected readonly searchText = signal('');

  protected readonly filteredRows = computed(() => {
    const filter = this.statusFilter();
    const keyword = this.searchText().trim().toLowerCase();
    return this.rows().filter((row) => {
      if (filter === 'published' && !row.isPublished) {
        return false;
      }
      if (filter === 'draft' && row.isPublished) {
        return false;
      }
      return keyword === '' || `${row.title ?? ''} ${row.linkUrl ?? ''}`.toLowerCase().includes(keyword);
    });
  });

  protected readonly canReorder = computed(
    () => this.statusFilter() === 'all' && this.searchText().trim() === '' && !this.loading(),
  );

  constructor() {
    this.statusControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.statusFilter.set(value ?? 'all'));
  }

  ngOnInit(): void {
    this.load();
  }

  protected clearFilter(): void {
    this.statusControl.setValue(null);
  }

  protected isFirst(index: number): boolean {
    return index === 0;
  }

  protected isLast(index: number): boolean {
    return index === this.filteredRows().length - 1;
  }

  protected move(row: AdminHomeBanner, direction: 'up' | 'down'): void {
    this.api.move(row.id, direction).subscribe({
      next: () => this.load(),
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.list().subscribe({
      next: (rows) => {
        this.rows.set(rows);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }

  protected togglePublished(row: AdminHomeBanner): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่แบนเนอร์แล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.load();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminHomeBanner): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบแบนเนอร์นี้?',
      message: `"${row.title ?? 'แบนเนอร์นี้'}" จะถูกลบออกจากหน้าแรก`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบแบนเนอร์แล้ว');
        this.load();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
