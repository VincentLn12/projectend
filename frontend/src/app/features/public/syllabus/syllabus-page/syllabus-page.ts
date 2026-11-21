import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FilesApi } from '../../../../core/files/api/files.api';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { SyllabusApi } from '../api/syllabus.api';
import { SUBJECT_CATEGORY_LABELS, SUBJECT_CATEGORY_ORDER, SubjectGroup, SyllabusDetail, SyllabusListItem } from '../types/syllabus.types';

const GROUP_COLORS = ['bg-[#1c5bb8]', 'bg-[#f7be33]', 'bg-[#0f2854]'];
const GROUP_STROKES = ['stroke-[#1c5bb8]', 'stroke-[#f7be33]', 'stroke-[#0f2854]'];

// Categories not in the list go last, in number order.
function displayPosition(category: number): number {
  const position = SUBJECT_CATEGORY_ORDER.indexOf(category);
  return position === -1 ? SUBJECT_CATEGORY_ORDER.length + category : position;
}

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
    const categories = [...new Set(subjects.map((subject) => subject.subjectCategory))].sort((a, b) => displayPosition(a) - displayPosition(b));
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

  protected readonly creditsInGroups = computed(() => this.groups().reduce((sum, group) => sum + group.totalCredits, 0));

  protected readonly chartSegments = computed(() => {
    const total = this.creditsInGroups();
    let accumulated = 0;
    return this.groups().map((group) => {
      const percent = total > 0 ? (group.totalCredits / total) * 100 : 0;
      const segment = {
        category: group.category,
        label: group.label,
        credits: group.totalCredits,
        roundedPercent: Math.round(percent),
        dashArray: `${percent} ${100 - percent}`,
        dashOffset: 25 - accumulated,
      };
      accumulated += percent;
      return segment;
    });
  });

  protected readonly selectedCategory = signal<number | null>(null);

  protected readonly visibleGroups = computed(() => {
    const selected = this.selectedCategory();
    return selected === null ? this.groups() : this.groups().filter((group) => group.category === selected);
  });

  protected readonly chartCenter = computed(() => {
    const selected = this.groups().find((group) => group.category === this.selectedCategory());
    return selected
      ? { credits: selected.totalCredits, caption: selected.label }
      : { credits: this.creditsInGroups(), caption: 'หน่วยกิต' };
  });

  protected toggleCategory(category: number): void {
    this.selectedCategory.update((current) => (current === category ? null : category));
  }

  protected groupStroke(category: number): string {
    return GROUP_STROKES[category % GROUP_STROKES.length];
  }

  protected groupColor(category: number): string {
    return GROUP_COLORS[category % GROUP_COLORS.length];
  }

  protected groupPercent(credits: number): number {
    const total = this.creditsInGroups();
    return total > 0 ? Math.round((credits / total) * 100) : 0;
  }

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

  protected scrollToSection(sectionId: string, event: Event): void {
    event.preventDefault();
    document.getElementById(sectionId)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
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
