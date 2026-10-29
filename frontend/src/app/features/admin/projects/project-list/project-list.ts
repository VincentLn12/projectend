import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { createPagedList } from '../../../../shared/data/paged-list';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminProjectsApi } from '../api/projects.api';
import { AdminProjectListItem } from '../types/project.types';

@Component({
  selector: 'app-project-list',
  imports: [RouterLink, ReactiveFormsModule, Button, DataTable, CellTemplate, Pagination, SearchBox, SelectInput],
  templateUrl: './project-list.html',
})
export class ProjectList implements OnInit {
  private readonly api = inject(AdminProjectsApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminProjectListItem>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminProjectListItem>[] = [
    { key: 'nameTH', header: 'ชื่อโครงงาน' },
    { key: 'academicYear', header: 'ปีการศึกษา', value: (row) => row.academicYear },
    { key: 'studentName', header: 'นักศึกษา', value: (row) => row.studentName },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminProjectListItem) => row.id;

  private readonly academicYears = signal<number[]>([]);
  protected readonly yearOptions = computed<SelectOption<number>[]>(() =>
    this.academicYears().map((year) => ({ value: year, label: String(year) })),
  );
  protected readonly statusOptions: SelectOption<boolean>[] = [
    { value: true, label: 'เผยแพร่' },
    { value: false, label: 'ฉบับร่าง' },
  ];
  protected readonly yearControl = new FormControl<number | null>(null);
  protected readonly statusControl = new FormControl<boolean | null>(null);
  protected readonly hasFilters = computed(() => Object.keys(this.list.filters()).length > 0);

  constructor() {
    this.yearControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.list.setFilter('academicYear', value === null ? null : String(value)));
    this.statusControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.list.setFilter('isPublished', value === null ? null : String(value)));
  }

  ngOnInit(): void {
    this.list.load();
    this.api.academicYears().subscribe({ next: (years) => this.academicYears.set(years) });
  }

  protected clearFilters(): void {
    this.yearControl.setValue(null);
    this.statusControl.setValue(null);
  }

  protected togglePublished(row: AdminProjectListItem): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่โครงงานแล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminProjectListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบโครงงานนี้?',
      message: `"${row.nameTH}" จะถูกลบออกจากเว็บไซต์`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบโครงงานแล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
