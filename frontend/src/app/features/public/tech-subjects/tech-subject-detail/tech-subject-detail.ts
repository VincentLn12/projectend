import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { TechSubjectsApi } from '../api/tech-subjects.api';
import { TechSubjectDetail, TechSubjectListItem } from '../types/tech-subject.types';

const HEADING_MAX_LENGTH = 40;

const CALLOUT_PREFIX = /^พูดง่าย/;

interface DescriptionBlock {
  text: string;
  kind: 'heading' | 'lead' | 'callout' | 'paragraph';
}

@Component({
  selector: 'app-public-tech-subject-detail',
  imports: [RouterLink, DataState, FileImage],
  templateUrl: './tech-subject-detail.html',
})
export class PublicTechSubjectDetail {
  private readonly api = inject(TechSubjectsApi);
  private readonly route = inject(ActivatedRoute);
  private subjectId = '';

  protected readonly subject = signal<TechSubjectDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly switching = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly allSubjects = signal<TechSubjectListItem[]>([]);

  protected readonly selectedImageIndex = signal(0);

  protected readonly mainImageId = computed(() => {
    const ids = this.subject()?.imageFileIds ?? [];
    return ids[this.selectedImageIndex()] ?? ids[0] ?? null;
  });

  protected readonly currentPosition = computed(() => this.allSubjects().findIndex((entry) => entry.id === this.subject()?.id));

  protected readonly previousSubject = computed(() => this.allSubjects()[this.currentPosition() - 1] ?? null);

  protected readonly nextSubject = computed(() => {
    const position = this.currentPosition();
    return position < 0 ? null : (this.allSubjects()[position + 1] ?? null);
  });

  protected readonly descriptionBlocks = computed<DescriptionBlock[]>(() => {
    const description = this.subject()?.description ?? '';
    let leadAssigned = false;
    return description
      .split('\n')
      .map((line) => line.trim())
      .filter((line) => line.length > 0)
      .map((text): DescriptionBlock => {
        if (text.length <= HEADING_MAX_LENGTH && !/[.!]$/.test(text)) {
          return { text, kind: 'heading' };
        }
        if (CALLOUT_PREFIX.test(text)) {
          return { text, kind: 'callout' };
        }
        if (!leadAssigned) {
          leadAssigned = true;
          return { text, kind: 'lead' };
        }
        return { text, kind: 'paragraph' };
      });
  });

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed()).subscribe((params) => {
      this.subjectId = params.get('id') ?? '';
      this.load();
    });
  }

  protected load(): void {
    // Keep the current subject on screen while the next one loads, so switching in the sidebar does not flash the page.
    if (this.subject() === null) {
      this.loading.set(true);
    } else {
      this.switching.set(true);
    }
    this.error.set(null);

    if (this.allSubjects().length > 0) {
      this.loadSubject();
      return;
    }

    this.api.list().subscribe({
      next: (list) => {
        this.allSubjects.set(list);
        this.loadSubject();
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  private loadSubject(): void {
    const id = this.subjectId || this.allSubjects()[0]?.id;
    if (!id) {
      this.subject.set(null);
      this.loading.set(false);
      this.switching.set(false);
      return;
    }

    this.api.get(id).subscribe({
      next: (subject) => {
        this.subject.set(subject);
        this.selectedImageIndex.set(0);
        this.loading.set(false);
        this.switching.set(false);
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  private fail(error: unknown): void {
    this.loading.set(false);
    this.switching.set(false);
    this.error.set(describeHttpError(error));
  }
}
