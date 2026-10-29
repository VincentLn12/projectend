import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { TechSubjectsApi } from '../api/tech-subjects.api';
import { TechSubjectDetail } from '../types/tech-subject.types';

@Component({
  selector: 'app-public-tech-subject-detail',
  imports: [RouterLink, DataState, FileImage],
  templateUrl: './tech-subject-detail.html',
})
export class PublicTechSubjectDetail implements OnInit {
  private readonly api = inject(TechSubjectsApi);
  private readonly subjectId = inject(ActivatedRoute).snapshot.paramMap.get('id')!;

  protected readonly subject = signal<TechSubjectDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get(this.subjectId).subscribe({
      next: (subject) => {
        this.subject.set(subject);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }
}
