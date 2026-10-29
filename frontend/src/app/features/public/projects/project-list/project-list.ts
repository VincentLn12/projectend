import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { createPagedList } from '../../../../shared/data/paged-list';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { ProjectsApi } from '../api/projects.api';
import { ProjectListItem } from '../types/project.types';

const BUDDHIST_ERA_OFFSET = 543;
const YEARS_SHOWN = 8;

@Component({
  selector: 'app-public-project-list',
  imports: [RouterLink, ReactiveFormsModule, DataState, FileImage, Pagination, SearchBox, SelectInput],
  templateUrl: './project-list.html',
})
export class PublicProjectList implements OnInit {
  private readonly api = inject(ProjectsApi);

  protected readonly year = new FormControl<number | null>(null);
  protected readonly yearOptions: SelectOption<number | null>[] = this.buildYearOptions();

  protected readonly list = createPagedList<ProjectListItem>((query) => this.api.list(query, this.year.value), 9);

  constructor() {
    this.year.valueChanges.pipe(takeUntilDestroyed(inject(DestroyRef))).subscribe(() => this.list.setPage(1));
  }

  ngOnInit(): void {
    this.list.load();
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
