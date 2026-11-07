import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';
import { createPagedList } from '../../../../shared/data/paged-list';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { PublicStudentProjectsApi } from '../api/student-projects.api';
import { SEMESTER_LABELS, StudentProjectListItem } from '../types/student-project.types';

const BUDDHIST_ERA_OFFSET = 543;
const YEARS_SHOWN = 8;
const PAGE_SIZE = 12;
const MAX_TAGS = 3;

@Component({
  selector: 'app-public-student-project-list',
  imports: [RouterLink, ReactiveFormsModule, Button, DataState, FileImage, Pagination, SearchBox, SelectInput],
  templateUrl: './student-project-list.html',
})
export class PublicStudentProjectList implements OnInit {
  private readonly api = inject(PublicStudentProjectsApi);
  protected readonly auth = inject(AuthService);

  protected readonly year = new FormControl<number | null>(null);
  protected readonly semester = new FormControl<number | null>(null);
  protected readonly yearOptions: SelectOption<number | null>[] = this.buildYearOptions();
  protected readonly semesterOptions: SelectOption<number | null>[] = [
    { value: null, label: 'ทุกภาคเรียน' },
    ...Object.entries(SEMESTER_LABELS).map(([value, label]) => ({ value: Number(value), label })),
  ];
  protected readonly semesterLabels = SEMESTER_LABELS;

  protected readonly list = createPagedList<StudentProjectListItem>(
    (query) => this.api.list(query, this.year.value, this.semester.value),
    PAGE_SIZE,
  );

  constructor() {
    const destroyRef = inject(DestroyRef);
    this.year.valueChanges.pipe(takeUntilDestroyed(destroyRef)).subscribe(() => this.list.setPage(1));
    this.semester.valueChanges.pipe(takeUntilDestroyed(destroyRef)).subscribe(() => this.list.setPage(1));
  }

  ngOnInit(): void {
    this.list.load();
  }

  protected tags(technologies: string | null): string[] {
    return (technologies ?? '')
      .split(',')
      .map((word) => word.trim())
      .filter((word) => word.length > 0)
      .slice(0, MAX_TAGS);
  }

  private buildYearOptions(): SelectOption<number | null>[] {
    const currentYear = new Date().getFullYear() + BUDDHIST_ERA_OFFSET;
    const options: SelectOption<number | null>[] = [{ value: null, label: 'ทุกปีการศึกษา' }];
    for (let offset = 0; offset < YEARS_SHOWN; offset++) {
      options.push({ value: currentYear - offset, label: `ปีการศึกษา ${currentYear - offset}` });
    }
    return options;
  }
}
