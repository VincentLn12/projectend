import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { StudentProjectsApi } from '../api/student-projects.api';
import { MyProfile, MyStudentProject, SEMESTER_LABELS } from '../types/student-project.types';

@Component({
  selector: 'app-my-student-project-list',
  imports: [RouterLink, ReactiveFormsModule, Button, DataState, FileImage, Toggle],
  templateUrl: './student-project-list.html',
})
export class MyStudentProjectList implements OnInit {
  private readonly api = inject(StudentProjectsApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);
  protected readonly auth = inject(AuthService);

  protected readonly projects = signal<MyStudentProject[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly profile = signal<MyProfile | null>(null);

  protected readonly showProfile = new FormControl(false, { nonNullable: true });
  protected readonly semesterLabels = SEMESTER_LABELS;

  protected readonly groups = computed(() => {
    const byTerm = new Map<string, { label: string; items: MyStudentProject[] }>();
    for (const project of this.projects()) {
      const key = `${project.academicYear}-${project.semester}`;
      const group = byTerm.get(key) ?? { label: `ปีการศึกษา ${project.academicYear} ${SEMESTER_LABELS[project.semester]}`, items: [] };
      group.items.push(project);
      byTerm.set(key, group);
    }
    return Array.from(byTerm.values());
  });

  constructor() {
    this.showProfile.valueChanges.pipe(takeUntilDestroyed(inject(DestroyRef))).subscribe(() => this.changeShowProfile());
  }

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.list().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
    this.api.profile().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.showProfile.setValue(profile.showProfile, { emitEvent: false });
      },
    });
  }

  protected changeShowProfile(): void {
    const profile = this.profile();
    if (!profile) {
      return;
    }
    const wanted = this.showProfile.value;
    this.api.setShowProfile(profile, wanted).subscribe({
      next: (updated) => {
        this.profile.set(updated);
        this.toast.success(wanted ? 'เปิดหน้า portfolio แล้ว' : 'ปิดหน้า portfolio แล้ว');
      },
      error: (error: unknown) => {
        this.showProfile.setValue(!wanted, { emitEvent: false });
        this.toast.error(describeHttpError(error));
      },
    });
  }

  protected togglePublished(project: MyStudentProject): void {
    const publish = !project.isPublished;
    this.api.setPublished(project.id, publish).subscribe({
      next: (updated) => {
        this.projects.update((list) => list.map((item) => (item.id === updated.id ? updated : item)));
        this.toast.success(publish ? 'เผยแพร่โปรเจคแล้ว' : 'ซ่อนโปรเจคแล้ว');
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(project: MyStudentProject): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบโปรเจคนี้?',
      message: `"${project.title}" จะถูกลบออกจากหน้า portfolio ของคุณ`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(project.id).subscribe({
      next: () => {
        this.toast.success('ลบโปรเจคแล้ว');
        this.projects.update((list) => list.filter((item) => item.id !== project.id));
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
