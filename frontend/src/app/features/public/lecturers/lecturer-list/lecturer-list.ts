import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { LecturersApi } from '../api/lecturers.api';
import { Lecturer } from '../types/lecturer.types';

const ACADEMIC_TITLE_SUFFIX = 'ศาสตราจารย์';
const CHAIR_KEYWORD = 'ประธาน';
const COMMITTEE_KEYWORD = 'กรรมการ';
const SECRETARY_KEYWORD = 'เลขานุการ';

interface LecturerGroup {
  title: string;
  lecturers: Lecturer[];
}

interface ExpertiseSection {
  title: string;
  items: string[];
}

@Component({
  selector: 'app-public-lecturer-list',
  imports: [RouterLink, DataState, FileImage],
  templateUrl: './lecturer-list.html',
})
export class PublicLecturerList implements OnInit {
  private readonly api = inject(LecturersApi);

  protected readonly lecturers = signal<Lecturer[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly featured = computed(() => this.lecturers().find((lecturer) => (lecturer.position ?? '').includes(CHAIR_KEYWORD)) ?? null);

  protected readonly others = computed(() => this.lecturers().filter((lecturer) => lecturer.id !== this.featured()?.id));

  protected readonly groups = computed<LecturerGroup[]>(() => {
    const others = this.others();
    const positionOf = (lecturer: Lecturer): string => lecturer.position ?? '';
    const committee = others.filter((lecturer) => positionOf(lecturer).includes(COMMITTEE_KEYWORD));
    const secretary = others.filter((lecturer) => positionOf(lecturer).includes(SECRETARY_KEYWORD) && !committee.includes(lecturer));
    const support = others.filter((lecturer) => !committee.includes(lecturer) && !secretary.includes(lecturer));
    return [
      { title: 'กรรมการหลักสูตร', lecturers: committee },
      { title: 'เลขานุการหลักสูตร', lecturers: secretary },
      { title: 'บุคลากรสนับสนุน', lecturers: support },
    ].filter((group) => group.lecturers.length > 0);
  });

  protected sections(expertise: string | null): ExpertiseSection[] {
    const sections: ExpertiseSection[] = [];
    for (const rawLine of (expertise ?? '').split('\n')) {
      const line = rawLine.trim();
      if (line === '') {
        continue;
      }
      if (line.startsWith('-')) {
        const item = line.replace(/^-\s*/, '');
        if (sections.length === 0) {
          sections.push({ title: '', items: [] });
        }
        sections[sections.length - 1].items.push(item);
      } else {
        sections.push({ title: line, items: [] });
      }
    }
    return sections;
  }

  protected displayName(lecturer: Lecturer): string {
    const prefix = (lecturer.prefix ?? '').trim();
    if (prefix === '' || lecturer.fullName.startsWith(prefix)) {
      return lecturer.fullName;
    }
    const separator = prefix.endsWith(ACADEMIC_TITLE_SUFFIX) ? ' ' : '';
    return `${prefix}${separator}${lecturer.fullName}`;
  }

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.list().subscribe({
      next: (lecturers) => {
        this.lecturers.set(lecturers);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }
}
