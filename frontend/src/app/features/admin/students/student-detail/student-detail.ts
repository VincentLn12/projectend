import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { BackLink } from '../../../../shared/navigation/back-link';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { Tabs, TabItem } from '../../../../shared/ui/tabs/tabs';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { AdminStudentsApi } from '../api/students.api';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminStudent, SEMESTER_LABELS, StudentCourseProject, StudentJobHistory, StudentProject } from '../types/student.types';

@Component({
  selector: 'app-student-detail',
  imports: [RouterLink, ThaiDatePipe, BackLink, Button, DataState, FileImage, Tabs],
  templateUrl: './student-detail.html',
})
export class StudentDetail implements OnInit {
  private readonly api = inject(AdminStudentsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly studentId = this.route.snapshot.paramMap.get('id')!;

  protected readonly activeTab = signal<'course' | 'projects' | 'jobs'>(this.initialTab());
  protected readonly semesterLabels = SEMESTER_LABELS;
  protected readonly student = signal<AdminStudent | null>(null);
  private readonly jobs = signal<StudentJobHistory[]>([]);
  protected readonly projects = signal<StudentProject[]>([]);
  protected readonly courseProjects = signal<StudentCourseProject[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly sortedJobs = computed(() =>
    [...this.jobs()].sort((a, b) => b.startDate.localeCompare(a.startDate)),
  );

  protected readonly publishedCount = computed(() => this.jobs().filter((job) => job.isPublished).length);

  protected keywordTags(project: StudentProject): string[] {
    return (project.keyWords ?? '')
      .split(/[,/\n]+/)
      .map((keyword) => keyword.trim())
      .filter((keyword) => keyword.length > 0)
      .slice(0, 3);
  }

  protected readonly tabItems = computed<TabItem[]>(() => [
    { key: 'course', label: 'โครงงานนักศึกษา', count: this.courseProjects().length },
    { key: 'projects', label: 'โครงงานวิจัย', count: this.projects().length },
    { key: 'jobs', label: 'ประวัติการทำงาน', count: this.sortedJobs().length },
  ]);

  protected selectTab(tab: string): void {
    this.activeTab.set(tab === 'jobs' ? 'jobs' : tab === 'projects' ? 'projects' : 'course');
    this.router.navigate([], { relativeTo: this.route, queryParams: { tab: this.activeTab() }, replaceUrl: true });
  }

  ngOnInit(): void {
    this.load();
  }

  protected technologyTags(project: StudentCourseProject): string[] {
    return (project.technologies ?? '')
      .split(',')
      .map((word) => word.trim())
      .filter((word) => word.length > 0)
      .slice(0, 4);
  }

  protected toggleCourseProject(project: StudentCourseProject): void {
    const publish = !project.isPublished;
    this.api.setCourseProjectPublished(project.id, publish).subscribe({
      next: () => {
        this.courseProjects.update((list) => list.map((item) => (item.id === project.id ? { ...item, isPublished: publish } : item)));
        this.toast.success(publish ? 'เผยแพร่โปรเจคแล้ว' : 'ซ่อนโปรเจคแล้ว');
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async deleteCourseProject(project: StudentCourseProject): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบโปรเจคนี้?',
      message: `"${project.title}" จะถูกลบออกจาก portfolio ของนักศึกษา`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.deleteCourseProject(project.id).subscribe({
      next: () => {
        this.toast.success('ลบโปรเจคแล้ว');
        this.courseProjects.update((list) => list.filter((item) => item.id !== project.id));
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  private initialTab(): 'course' | 'projects' | 'jobs' {
    const tab = this.route.snapshot.queryParamMap.get('tab');
    return tab === 'jobs' ? 'jobs' : tab === 'projects' ? 'projects' : 'course';
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    forkJoin({
      student: this.api.get(this.studentId),
      jobs: this.api.jobHistories(this.studentId),
      projects: this.api.projects(this.studentId),
      courseProjects: this.api.courseProjects(this.studentId),
    }).subscribe({
      next: ({ student, jobs, projects, courseProjects }) => {
        this.student.set(student);
        this.jobs.set(jobs);
        this.projects.set(projects);
        this.courseProjects.set(courseProjects);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }
}
