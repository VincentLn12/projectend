import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { createPagedList } from '../../../../shared/data/paged-list';
import { Button } from '../../../../shared/ui/button/button';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { PageHeader } from '../../../../shared/ui/page-header/page-header';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { AdminStudentsApi } from '../api/students.api';
import { AdminStudent } from '../types/student.types';

@Component({
  selector: 'app-student-list',
  imports: [RouterLink, ReactiveFormsModule, Button, DataTable, CellTemplate, FileImage, PageHeader, Pagination, SearchBox, SelectInput],
  templateUrl: './student-list.html',
})
export class StudentList implements OnInit {
  private readonly api = inject(AdminStudentsApi);

  protected readonly list = createPagedList<AdminStudent>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminStudent>[] = [
    { key: 'fullName', header: 'ชื่อ-สกุล', sortable: true },
    { key: 'studentCode', header: 'รหัสนักศึกษา', value: (row) => row.studentCode, sortable: true },
    { key: 'entryYear', header: 'ปีที่เข้าเรียน', value: (row) => row.entryYear, sortable: true },
    { key: 'email', header: 'อีเมล', value: (row) => row.email },
    { key: 'status', header: 'สถานะบัญชี' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminStudent) => row.id;

  private readonly entryYears = signal<number[]>([]);
  protected readonly yearOptions = computed<SelectOption<number>[]>(() =>
    this.entryYears().map((year) => ({ value: year, label: String(year) })),
  );
  protected readonly statusOptions: SelectOption<boolean>[] = [
    { value: false, label: 'ใช้งานได้' },
    { value: true, label: 'ถูกล็อก' },
  ];
  protected readonly yearControl = new FormControl<number | null>(null);
  protected readonly statusControl = new FormControl<boolean | null>(null);
  protected readonly hasFilters = computed(() => Object.keys(this.list.filters()).length > 0);

  constructor() {
    this.yearControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.list.setFilter('entryYear', value === null ? null : String(value)));
    this.statusControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.list.setFilter('isLocked', value === null ? null : String(value)));
  }

  ngOnInit(): void {
    this.list.load();
    this.api.entryYears().subscribe({ next: (years) => this.entryYears.set(years) });
  }

  protected clearFilters(): void {
    this.yearControl.setValue(null);
    this.statusControl.setValue(null);
  }
}
