import { Component, computed, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { createPagedList } from '../../../../shared/data/paged-list';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { PageHeader } from '../../../../shared/ui/page-header/page-header';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminSyllabusesApi } from '../api/syllabuses.api';
import { AdminSyllabusListItem } from '../types/syllabus.types';

@Component({
  selector: 'app-syllabus-list',
  imports: [RouterLink, ReactiveFormsModule, Button, DataTable, CellTemplate, PageHeader, Pagination, SearchBox, SelectInput],
  templateUrl: './syllabus-list.html',
})
export class SyllabusList implements OnInit {
  private readonly api = inject(AdminSyllabusesApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminSyllabusListItem>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminSyllabusListItem>[] = [
    { key: 'nameTH', header: 'ชื่อหลักสูตร', sortable: true },
    { key: 'year', header: 'ฉบับปี พ.ศ.', value: (row) => row.year, sortable: true },
    { key: 'current', header: 'ฉบับปัจจุบัน' },
    { key: 'status', header: 'สถานะ', sortable: true },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminSyllabusListItem) => row.id;

  protected readonly statusOptions: SelectOption<boolean>[] = [
    { value: true, label: 'เผยแพร่' },
    { value: false, label: 'ฉบับร่าง' },
  ];
  protected readonly statusControl = new FormControl<boolean | null>(null);
  protected readonly hasFilters = computed(() => Object.keys(this.list.filters()).length > 0);

  constructor() {
    this.statusControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.list.setFilter('isPublished', value === null ? null : String(value)));
  }

  ngOnInit(): void {
    this.list.load();
  }

  protected clearFilters(): void {
    this.statusControl.setValue(null);
  }

  protected togglePublished(row: AdminSyllabusListItem): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่หลักสูตรแล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminSyllabusListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบหลักสูตรนี้?',
      message: `"${row.nameTH}" ฉบับ พ.ศ. ${row.year} พร้อมรายวิชาทั้งหมดจะถูกลบ`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบหลักสูตรแล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
