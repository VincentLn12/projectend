import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, Observable } from 'rxjs';
import { isoToLocalDateTime, localDateTimeToIso } from '../../../../shared/forms/date-time';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { BackLink } from '../../../../shared/navigation/back-link';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { DateInput } from '../../../../shared/ui/date-input/date-input';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { AdminShortCoursesApi } from '../api/short-courses.api';
import { AdminShortCourse, CertificateTemplateOption, SaveShortCourseRequest } from '../types/short-course.types';

@Component({
  selector: 'app-short-course-edit',
  imports: [ReactiveFormsModule, RouterLink, BackLink, Button, DataState, DateInput, FileUpload, SelectInput, TextArea, TextInput, Toggle],
  templateUrl: './short-course-edit.html',
})
export class ShortCourseEdit implements OnInit {
  private readonly api = inject(AdminShortCoursesApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly courseId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.courseId === null;

  protected readonly form = inject(FormBuilder).group({
    title: ['', [Validators.required, Validators.maxLength(300)]],
    description: [''],
    audience: ['', Validators.maxLength(500)],
    location: ['', Validators.maxLength(500)],
    teacherName: ['', Validators.maxLength(300)],
    startDate: ['' as string | null, Validators.required],
    endDate: ['' as string | null, Validators.required],
    hours: [null as number | null, [Validators.min(1), Validators.max(1000)]],
    maxParticipants: [30 as number | null, [Validators.required, Validators.min(0), Validators.max(100000)]],
    registrationOpenAt: ['' as string | null],
    registrationCloseAt: ['' as string | null],
    isRegistrationEnabled: [true],
    isPublished: [false],
    coverFileId: [null as string | null],
    certificateTemplateId: [null as string | null],
  });

  protected readonly templates = signal<CertificateTemplateOption[]>([]);
  protected readonly templateOptions = computed<SelectOption<string>[]>(() =>
    this.templates().map((template) => ({ value: template.id, label: template.name })),
  );

  protected readonly registeredCount = signal(0);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly submitted = signal(false);
  protected readonly formError = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    const requests: Record<string, Observable<unknown>> = { templates: this.api.certificateTemplates() };
    if (!this.isNew) {
      requests['course'] = this.api.get(this.courseId!);
    }
    forkJoin(requests).subscribe({
      next: (result) => {
        this.templates.set(result['templates'] as CertificateTemplateOption[]);
        if (result['course']) {
          this.fillForm(result['course'] as AdminShortCourse);
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
    this.submitted.set(true);

    const value = this.form.getRawValue();
    if (value.startDate && value.endDate && new Date(value.endDate) < new Date(value.startDate)) {
      this.form.controls.endDate.setErrors({ server: 'วันสิ้นสุดต้องไม่ก่อนวันเริ่มอบรม' });
    }
    if (value.registrationOpenAt && value.registrationCloseAt && new Date(value.registrationCloseAt) < new Date(value.registrationOpenAt)) {
      this.form.controls.registrationCloseAt.setErrors({ server: 'วันปิดรับสมัครต้องไม่ก่อนวันเปิดรับสมัคร' });
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
      next: () => {
        this.toast.success('บันทึกหลักสูตรแล้ว');
        this.router.navigateByUrl('/admin/short-courses');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(course: AdminShortCourse): void {
    this.registeredCount.set(course.registeredCount);
    this.form.reset({
      title: course.title,
      description: course.description ?? '',
      audience: course.audience ?? '',
      location: course.location ?? '',
      teacherName: course.teacherName ?? '',
      startDate: isoToLocalDateTime(course.startDate),
      endDate: isoToLocalDateTime(course.endDate),
      hours: course.hours,
      maxParticipants: course.maxParticipants,
      registrationOpenAt: isoToLocalDateTime(course.registrationOpenAt),
      registrationCloseAt: isoToLocalDateTime(course.registrationCloseAt),
      isRegistrationEnabled: course.isRegistrationEnabled,
      isPublished: course.isPublished,
      coverFileId: course.coverFileId,
      certificateTemplateId: course.certificateTemplateId,
    });
  }

  private buildRequest(): SaveShortCourseRequest {
    const value = this.form.getRawValue();
    return {
      title: (value.title ?? '').trim(),
      description: (value.description ?? '').trim() || null,
      audience: (value.audience ?? '').trim() || null,
      location: (value.location ?? '').trim() || null,
      teacherName: (value.teacherName ?? '').trim() || null,
      startDate: localDateTimeToIso(value.startDate)!,
      endDate: localDateTimeToIso(value.endDate)!,
      hours: value.hours,
      maxParticipants: value.maxParticipants ?? 0,
      registrationOpenAt: localDateTimeToIso(value.registrationOpenAt),
      registrationCloseAt: localDateTimeToIso(value.registrationCloseAt),
      isRegistrationEnabled: value.isRegistrationEnabled ?? false,
      isPublished: value.isPublished ?? false,
      coverFileId: value.coverFileId,
      certificateTemplateId: value.certificateTemplateId,
    };
  }
}
