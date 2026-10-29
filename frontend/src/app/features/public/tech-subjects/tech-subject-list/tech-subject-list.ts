import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { TechSubjectsApi } from '../api/tech-subjects.api';
import { TechSubjectListItem } from '../types/tech-subject.types';

@Component({
  selector: 'app-public-tech-subject-list',
  imports: [RouterLink, DataState, FileImage],
  templateUrl: './tech-subject-list.html',
})
export class PublicTechSubjectList implements OnInit {
  private readonly api = inject(TechSubjectsApi);

  protected readonly subjects = signal<TechSubjectListItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.list().subscribe({
      next: (subjects) => {
        this.subjects.set(subjects);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }
}
