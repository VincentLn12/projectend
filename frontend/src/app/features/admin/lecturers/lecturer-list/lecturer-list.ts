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
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { PageHeader } from '../../../../shared/ui/page-header/page-header';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminLecturersApi } from '../api/lecturers.api';
import { AdminLecturer } from '../types/lecturer.types';

@Component({
  selector: 'app-lecturer-list',
  imports: [RouterLink, ReactiveFormsModule, Button, DataTable, CellTemplate, FileImage, PageHeader, Pagination, SearchBox, SelectInput],
  templateUrl: './lecturer-list.html',
})
export class LecturerList implements OnInit {
  private readonly api = inject(AdminLecturersApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminLecturer>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminLecturer>[] = [
    { key: 'fullName', header: 'ชื่อ-สกุล', sortable: true },
    { key: 'position', header: 'ตำแหน่ง', value: (row) => row.position, sortable: true },
    { key: 'sortOrder', header: 'ลำดับ', value: (row) => row.sortOrder, sortable: true },
    { key: 'status', header: 'สถานะ', sortable: true },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminLecturer) => row.id;

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

  protected readonly canReorder = computed(
    () => this.list.sort() === null && !this.hasFilters() && !this.list.search() && !this.list.loading(),
  );

  protected isFirst(index: number): boolean {
    return this.list.page() === 1 && index === 0;
  }

  protected isLast(index: number): boolean {
    return (this.list.page() - 1) * this.list.pageSize + index + 1 >= this.list.total();
  }

  protected move(row: AdminLecturer, direction: 'up' | 'down'): void {
    this.api.move(row.id, direction).subscribe({
      next: () => this.list.reload(),
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected clearFilters(): void {
    this.statusControl.setValue(null);
  }

  protected togglePublished(row: AdminLecturer): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่อาจารย์แล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminLecturer): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบอาจารย์นี้?',
      message: `"${row.prefix ?? ''}${row.fullName}" จะถูกลบออกจากหน้าอาจารย์ของเว็บไซต์`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบอาจารย์แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
