import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { PublicStudentsApi } from '../api/students.api';
import { PortfolioStudentProject, SEMESTER_LABELS, StudentPortfolio } from '../types/student.types';

@Component({
  selector: 'app-student-portfolio',
  imports: [RouterLink, DataState, FileImage],
  templateUrl: './student-portfolio.html',
})
export class StudentPortfolioPage implements OnInit {
  private readonly api = inject(PublicStudentsApi);
  private readonly studentId = inject(ActivatedRoute).snapshot.paramMap.get('id')!;

  protected readonly student = signal<StudentPortfolio | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly terms = computed(() => {
    const byTerm = new Map<string, { label: string; items: PortfolioStudentProject[] }>();
    for (const project of this.student()?.studentProjects ?? []) {
      const key = `${project.academicYear}-${project.semester}`;
      const term = byTerm.get(key) ?? { label: `ปีการศึกษา ${project.academicYear} ${SEMESTER_LABELS[project.semester]}`, items: [] };
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
}
