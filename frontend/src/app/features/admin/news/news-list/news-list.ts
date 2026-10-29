import { Component, computed, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { createPagedList } from '../../../../shared/data/paged-list';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { DateInput } from '../../../../shared/ui/date-input/date-input';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminNewsApi } from '../api/news.api';
import { AdminNewsListItem } from '../types/news.types';

@Component({
  selector: 'app-news-list',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    ThaiDatePipe,
    Button,
    DataTable,
    CellTemplate,
    DateInput,
    Pagination,
    SearchBox,
    SelectInput,
  ],
  templateUrl: './news-list.html',
})
export class NewsList implements OnInit {
  private readonly api = inject(AdminNewsApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminNewsListItem>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminNewsListItem>[] = [
    { key: 'title', header: 'หัวข้อ', sortable: true },
    { key: 'status', header: 'สถานะ', sortable: true },
    { key: 'publishedAt', header: 'วันที่เผยแพร่', sortable: true },
    { key: 'authorName', header: 'ผู้เขียน', value: (row) => row.authorName, sortable: true },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly statusOptions: SelectOption<boolean>[] = [
    { value: true, label: 'เผยแพร่' },
    { value: false, label: 'ฉบับร่าง' },
  ];
  protected readonly statusControl = new FormControl<boolean | null>(null);
  protected readonly publishedFromControl = new FormControl<string | null>(null);
  protected readonly publishedToControl = new FormControl<string | null>(null);

  protected readonly hasFilters = computed(() => Object.keys(this.list.filters()).length > 0);

  protected readonly rowKey = (row: AdminNewsListItem) => row.id;

  constructor() {
    this.statusControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.list.setFilter('isPublished', value === null ? null : String(value)));
    this.publishedFromControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.list.setFilter('publishedFrom', value));
    this.publishedToControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.list.setFilter('publishedTo', value));
  }

  ngOnInit(): void {
    this.list.load();
  }

  protected clearFilters(): void {
    this.statusControl.setValue(null);
    this.publishedFromControl.setValue(null);
    this.publishedToControl.setValue(null);
  }

  protected togglePublished(row: AdminNewsListItem): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่ข่าวแล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminNewsListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบข่าวนี้?',
      message: `"${row.title}" จะถูกลบออกจากเว็บไซต์`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบข่าวแล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
