import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FilesApi } from '../../../../core/files/api/files.api';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { SyllabusApi } from '../api/syllabus.api';
import { SUBJECT_CATEGORY_LABELS, SubjectGroup, SyllabusDetail, SyllabusListItem } from '../types/syllabus.types';

@Component({
  selector: 'app-public-syllabus',
  imports: [RouterLink, Button, DataState],
  templateUrl: './syllabus-page.html',
})
export class PublicSyllabus {
  private readonly api = inject(SyllabusApi);
  private readonly filesApi = inject(FilesApi);

  private readonly route = inject(ActivatedRoute);
  private requestedId: string | null = null;

  protected readonly syllabus = signal<SyllabusDetail | null>(null);
  protected readonly others = signal<SyllabusListItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly groups = computed<SubjectGroup[]>(() => {
    const subjects = this.syllabus()?.subjects ?? [];
    const categories = [...new Set(subjects.map((subject) => subject.subjectCategory))].sort((a, b) => a - b);
    return categories.map((category) => {
      const inCategory = subjects.filter((subject) => subject.subjectCategory === category);
      return {
        category,
        label: SUBJECT_CATEGORY_LABELS[category] ?? `หมวดที่ ${category}`,
        subjects: inCategory,
        totalCredits: inCategory.reduce((sum, subject) => sum + subject.credit, 0),
      };
    });
  });

  protected readonly pdfUrl = computed(() => {
    const id = this.syllabus()?.pdfFileId;
    return id ? this.filesApi.url(id) : null;
  });

  constructor() {
    this.api.list().subscribe({ next: (list) => this.others.set(list), error: () => undefined });

    // /syllabus shows the current syllabus, /syllabus/:id a specific one. The same component stays
    // on screen when only the id changes, so every change of the route loads again.
    this.route.paramMap.pipe(takeUntilDestroyed()).subscribe((params) => {
      this.requestedId = params.get('id');
      this.load();
    });
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    const id = this.requestedId;
    (id ? this.api.get(id) : this.api.current()).subscribe({
      next: (syllabus) => {
        this.syllabus.set(syllabus);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }
}
