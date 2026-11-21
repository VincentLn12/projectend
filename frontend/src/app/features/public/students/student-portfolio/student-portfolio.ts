import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { PublicStudentsApi } from '../api/students.api';
import { PortfolioStudentProject, SEMESTER_LABELS, StudentPortfolio } from '../types/student.types';

interface Skill {
  name: string;
  count: number;
}

@Component({
  selector: 'app-student-portfolio',
  imports: [RouterLink, ThaiDatePipe, Button, DataState, FileImage],
  templateUrl: './student-portfolio.html',
})
export class StudentPortfolioPage implements OnInit {
  private readonly api = inject(PublicStudentsApi);
  private readonly toast = inject(ToastService);
  private readonly studentId = inject(ActivatedRoute).snapshot.paramMap.get('id')!;

  protected readonly student = signal<StudentPortfolio | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly semesterLabels = SEMESTER_LABELS;

  protected readonly initial = computed(() => (Array.from(this.student()?.fullName.replace(/^(นาย|นางสาว|นาง)/, '').trim() ?? '')[0] ?? '?').toUpperCase());

  protected readonly profileSkills = computed(() => this.splitTags(this.student()?.skills ?? null));

  protected readonly skills = computed<Skill[]>(() => {
    const counts = new Map<string, number>();
    for (const project of this.student()?.studentProjects ?? []) {
      for (const name of this.splitTags(project.technologies)) {
        counts.set(name, (counts.get(name) ?? 0) + 1);
      }
    }
    return Array.from(counts, ([name, count]) => ({ name, count })).sort((a, b) => b.count - a.count || a.name.localeCompare(b.name));
  });

  protected readonly stats = computed(() => {
    const item = this.student();
    const projects = item?.studentProjects ?? [];
    const terms = new Set(projects.map((project) => `${project.academicYear}-${project.semester}`));
    return [
      { value: projects.length, label: 'โครงงานระหว่างเรียน' },
      { value: terms.size, label: 'ภาคเรียนที่มีผลงาน' },
      { value: this.skills().length, label: 'เทคโนโลยีที่ใช้' },
      { value: item?.projects.length ?? 0, label: 'โครงงานวิจัย (โปรเจคจบ)' },
    ];
  });

  protected readonly terms = computed(() => {
    const byTerm = new Map<string, { label: string; items: PortfolioStudentProject[] }>();
    for (const project of this.student()?.studentProjects ?? []) {
      const key = `${project.academicYear}-${project.semester}`;
      const term = byTerm.get(key) ?? { label: `ปีการศึกษา ${project.academicYear} · ${SEMESTER_LABELS[project.semester]}`, items: [] };
      term.items.push(project);
      byTerm.set(key, term);
    }
    return Array.from(byTerm.values());
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get(this.studentId).subscribe({
      next: (student) => {
        this.student.set(student);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }

  protected tags(technologies: string | null): string[] {
    return this.splitTags(technologies);
  }

  protected print(): void {
    window.print();
  }

  protected async copyLink(): Promise<void> {
    try {
      await navigator.clipboard.writeText(window.location.href);
      this.toast.success('คัดลอกลิงก์ portfolio แล้ว');
    } catch {
      this.toast.info('คัดลอกไม่ได้ กรุณาคัดลอกจากแถบที่อยู่เว็บ');
    }
  }

  private splitTags(technologies: string | null): string[] {
    return (technologies ?? '')
      .split(',')
      .map((word) => word.trim())
      .filter((word) => word.length > 0);
  }
}
