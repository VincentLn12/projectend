import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { forkJoin, Observable, startWith } from 'rxjs';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { urlValidator } from '../../../../shared/forms/validators';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { StudentProjectsApi } from '../api/student-projects.api';
import { LecturerOption, MyStudentProject, SaveStudentProjectRequest, SEMESTER_LABELS } from '../types/student-project.types';

const BUDDHIST_ERA_OFFSET = 543;
const YEARS_AHEAD = 1;
const YEARS_BACK = 8;

@Component({
  selector: 'app-my-student-project-edit',
  imports: [ReactiveFormsModule, RouterLink, Button, DataState, FileUpload, SelectInput, TextArea, TextInput, Toggle],
  templateUrl: './student-project-edit.html',
})
export class MyStudentProjectEdit implements OnInit {
  private readonly api = inject(StudentProjectsApi);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly projectId = inject(ActivatedRoute).snapshot.paramMap.get('id');

  protected readonly isNew = this.projectId === null;

  private readonly currentYear = new Date().getFullYear() + BUDDHIST_ERA_OFFSET;
  private readonly currentSemester = new Date().getMonth() >= 5 && new Date().getMonth() <= 9 ? 1 : 2;

  protected readonly form = inject(FormBuilder).group({
    title: ['', [Validators.required, Validators.maxLength(300)]],
    academicYear: [this.currentYear as number | null, Validators.required],
    semester: [this.currentSemester as number | null, Validators.required],
    courseName: ['', Validators.maxLength(200)],
    description: [''],
    technologies: ['', Validators.maxLength(500)],
    lecturerId: [null as string | null],
    imageFileId: [null as string | null],
    pdfFileId: [null as string | null],
    videoUrl: ['', [Validators.maxLength(500), urlValidator]],
    webUrl: ['', [Validators.maxLength(500), urlValidator]],
    githubUrl: ['', [Validators.maxLength(500), urlValidator]],
    isPublished: [true],
  });

  protected readonly yearOptions: SelectOption<number>[] = Array.from({ length: YEARS_AHEAD + YEARS_BACK + 1 }, (_, index) => {
    const year = this.currentYear + YEARS_AHEAD - index;
    return { value: year, label: `${year}` };
  });

  protected readonly semesterOptions: SelectOption<number>[] = Object.entries(SEMESTER_LABELS).map(([value, label]) => ({
    value: Number(value),
    label,
  }));

  protected readonly lecturers = signal<LecturerOption[]>([]);
  protected readonly lecturerOptions = computed<SelectOption<string>[]>(() =>
    this.lecturers().map((lecturer) => ({ value: lecturer.id, label: lecturer.label })),
  );

  private readonly otherProjects = signal<MyStudentProject[]>([]);
  private readonly formValue = toSignal(this.form.valueChanges.pipe(startWith(this.form.getRawValue())), {
    initialValue: this.form.getRawValue(),
  });

  protected readonly sameTermProject = computed(() => {
    const value = this.formValue();
    return this.otherProjects().find((project) => project.academicYear === value.academicYear && project.semester === value.semester) ?? null;
  });

  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    const requests: Record<string, Observable<unknown>> = { lecturers: this.api.lecturers(), mine: this.api.list() };
    if (!this.isNew) {
      requests['project'] = this.api.get(this.projectId!);
    }
    forkJoin(requests).subscribe({
      next: (result) => {
        this.lecturers.set(result['lecturers'] as LecturerOption[]);
        this.otherProjects.set((result['mine'] as MyStudentProject[]).filter((project) => project.id !== this.projectId));
        if (result['project']) {
          this.fillForm(result['project'] as MyStudentProject);
        }
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.loadError.set(describeHttpError(error));
      },
    });
  }

  protected save(): void {
    if (this.saving()) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.formError.set(null);

    const request = this.buildRequest();
    const call = this.isNew ? this.api.create(request) : this.api.update(this.projectId!, request);
    call.subscribe({
      next: () => {
        this.toast.success('บันทึกโปรเจคแล้ว');
        this.router.navigateByUrl('/my/student-projects');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(project: MyStudentProject): void {
    this.form.reset({
      title: project.title,
      academicYear: project.academicYear,
      semester: project.semester,
      courseName: project.courseName ?? '',
      description: project.description ?? '',
      technologies: project.technologies ?? '',
      lecturerId: project.lecturerId,
      imageFileId: project.imageFileId,
      pdfFileId: project.pdfFileId,
      videoUrl: project.videoUrl ?? '',
      webUrl: project.webUrl ?? '',
      githubUrl: project.githubUrl ?? '',
      isPublished: project.isPublished,
    });
  }

  private buildRequest(): SaveStudentProjectRequest {
    const value = this.form.getRawValue();
    return {
      title: (value.title ?? '').trim(),
      description: (value.description ?? '').trim() || null,
      academicYear: value.academicYear!,
      semester: value.semester!,
      courseName: (value.courseName ?? '').trim() || null,
      technologies: (value.technologies ?? '').trim() || null,
      lecturerId: value.lecturerId,
      imageFileId: value.imageFileId,
      pdfFileId: value.pdfFileId,
      videoUrl: (value.videoUrl ?? '').trim() || null,
      webUrl: (value.webUrl ?? '').trim() || null,
      githubUrl: (value.githubUrl ?? '').trim() || null,
      isPublished: value.isPublished ?? false,
    };
  }
}
