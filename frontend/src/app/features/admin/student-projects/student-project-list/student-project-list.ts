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
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminStudentProjectsApi } from '../api/student-projects.api';
import { AdminStudentProjectListItem, SEMESTER_LABELS } from '../types/student-project.types';

@Component({
  selector: 'app-admin-student-project-list',
  imports: [RouterLink, ReactiveFormsModule, Button, DataTable, CellTemplate, Pagination, SearchBox, SelectInput],
  templateUrl: './student-project-list.html',
})
export class AdminStudentProjectList implements OnInit {
  private readonly api = inject(AdminStudentProjectsApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminStudentProjectListItem>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminStudentProjectListItem>[] = [
    { key: 'title', header: 'ชื่อโปรเจค' },
    { key: 'studentName', header: 'นักศึกษา', value: (row) => row.studentName },
    { key: 'term', header: 'ปีการศึกษา / ภาคเรียน', value: (row) => `${row.academicYear} · ${SEMESTER_LABELS[row.semester]}` },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminStudentProjectListItem) => row.id;

  protected readonly yearOptions: SelectOption<number>[] = this.buildYearOptions();
  protected readonly semesterOptions: SelectOption<number>[] = Object.entries(SEMESTER_LABELS).map(([value, label]) => ({
    value: Number(value),
    label,
  }));
  protected readonly statusOptions: SelectOption<boolean>[] = [
    { value: true, label: 'เผยแพร่' },
    { value: false, label: 'ซ่อน / ฉบับร่าง' },
  ];

  protected readonly yearControl = new FormControl<number | null>(null);
  protected readonly semesterControl = new FormControl<number | null>(null);
  protected readonly statusControl = new FormControl<boolean | null>(null);
  protected readonly hasFilters = computed(() => Object.keys(this.list.filters()).length > 0);

  constructor() {
    this.yearControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.list.setFilter('academicYear', value === null ? null : String(value)));
    this.semesterControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.list.setFilter('semester', value === null ? null : String(value)));
    this.statusControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.list.setFilter('isPublished', value === null ? null : String(value)));
  }

  ngOnInit(): void {
    this.list.load();
  }

  protected clearFilters(): void {
    this.yearControl.setValue(null);
    this.semesterControl.setValue(null);
    this.statusControl.setValue(null);
  }

  protected togglePublished(row: AdminStudentProjectListItem): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่โปรเจคแล้ว' : 'ซ่อนโปรเจคแล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminStudentProjectListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบโปรเจคนี้?',
      message: `"${row.title}" ของ ${row.studentName} จะถูกลบ`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบโปรเจคแล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  private buildYearOptions(): SelectOption<number>[] {
    const currentYear = new Date().getFullYear() + 543;
    return Array.from({ length: 8 }, (_, index) => ({ value: currentYear - index, label: String(currentYear - index) }));
  }
}
