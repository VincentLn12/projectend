import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, Observable } from 'rxjs';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { AdminCoursesApi } from '../api/courses.api';
import { AdminCourse, Generation, PickerOption, SaveCourseRequest } from '../types/course.types';

@Component({
  selector: 'app-course-edit',
  imports: [ReactiveFormsModule, RouterLink, ThaiDatePipe, Button, DataState, FileUpload, SelectInput, TextArea, TextInput, Toggle],
  templateUrl: './course-edit.html',
})
export class CourseEdit implements OnInit {
  private readonly api = inject(AdminCoursesApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly courseId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.courseId === null;

  protected readonly form = inject(FormBuilder).group({
    title: ['', [Validators.required, Validators.maxLength(300)]],
    description: [''],
    lecturerId: [null as string | null],
    fileIds: [[] as string[]],
    isPublished: [false],
  });

  protected readonly lecturers = signal<PickerOption[]>([]);
  protected readonly lecturerOptions = computed<SelectOption<string>[]>(() =>
    this.lecturers().map((lecturer) => ({ value: lecturer.id, label: lecturer.label })),
  );
  protected readonly generations = signal<Generation[]>([]);

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

    const requests: Record<string, Observable<unknown>> = { lecturers: this.api.lecturers() };
    if (!this.isNew) {
      requests['course'] = this.api.get(this.courseId!);
    }
    forkJoin(requests).subscribe({
      next: (result) => {
        this.lecturers.set(result['lecturers'] as PickerOption[]);
        if (result['course']) {
          this.fillForm(result['course'] as AdminCourse);
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
    const call = this.isNew ? this.api.create(request) : this.api.update(this.courseId!, request);
    call.subscribe({
      next: (saved) => {
        this.toast.success('บันทึกหลักสูตรแล้ว');
        // A new course goes straight to its own page so that generations can be added.
        this.router.navigateByUrl(this.isNew ? `/admin/courses/${saved.id}` : '/admin/courses');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  protected seatsText(generation: Generation): string {
    return `${generation.registeredCount} / ${generation.quantity}`;
  }

  protected async deleteGeneration(generation: Generation): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบรุ่นอบรมนี้?',
      message: `"${generation.subTitle ?? 'รุ่นอบรม'}" จะถูกลบ รวมถึงรายชื่อผู้ลงทะเบียน`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.deleteGeneration(generation.id).subscribe({
      next: () => {
        this.toast.success('ลบรุ่นอบรมแล้ว');
        this.generations.update((list) => list.filter((item) => item.id !== generation.id));
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  private fillForm(course: AdminCourse): void {
    // The first id is the main picture, as in the upload component.
    const ordered = [...course.photos].sort((a, b) => Number(b.isMain) - Number(a.isMain));
    this.generations.set(course.generations);
    this.form.reset({
      title: course.title,
      description: course.description ?? '',
      lecturerId: course.lecturerId,
      fileIds: ordered.map((photo) => photo.fileId),
      isPublished: course.isPublished,
    });
  }

  private buildRequest(): SaveCourseRequest {
    const value = this.form.getRawValue();
    const fileIds = value.fileIds ?? [];
    return {
      title: (value.title ?? '').trim(),
      description: (value.description ?? '').trim() || null,
      lecturerId: value.lecturerId,
      isPublished: value.isPublished ?? false,
      fileIds,
      mainFileId: fileIds[0] ?? null,
    };
  }
}
