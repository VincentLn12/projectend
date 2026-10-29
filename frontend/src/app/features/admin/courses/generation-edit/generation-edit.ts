import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { isoToLocalDateTime, localDateTimeToIso } from '../../../../shared/forms/date-time';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { DateInput } from '../../../../shared/ui/date-input/date-input';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { AdminCoursesApi } from '../api/courses.api';
import { Generation, SaveGenerationRequest } from '../types/course.types';

@Component({
  selector: 'app-generation-edit',
  imports: [ReactiveFormsModule, RouterLink, Button, DataState, DateInput, FileUpload, TextArea, TextInput, Toggle],
  templateUrl: './generation-edit.html',
})
export class GenerationEdit implements OnInit {
  private readonly api = inject(AdminCoursesApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly courseId = this.route.snapshot.paramMap.get('courseId')!;
  protected readonly generationId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.generationId === null;

  protected readonly form = inject(FormBuilder).group({
    subTitle: ['', Validators.maxLength(300)],
    description: [''],
    startDate: ['' as string | null, Validators.required],
    endDate: ['' as string | null, Validators.required],
    quantity: [20 as number | null, [Validators.required, Validators.min(1), Validators.max(100000)]],
    fee: [null as number | null, [Validators.min(0), Validators.max(10000000)]],
    photoFileId: [null as string | null],
    isCancelled: [false],
  });

  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  ngOnInit(): void {
    if (!this.isNew) {
      this.load();
    }
  }

  protected load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    // There is no endpoint for a single generation: it comes with its course.
    this.api.get(this.courseId).subscribe({
      next: (course) => {
        const generation = course.generations.find((item) => item.id === this.generationId);
        if (generation) {
          this.fillForm(generation);
          this.loading.set(false);
        } else {
          this.loading.set(false);
          this.loadError.set('ไม่พบรุ่นอบรมที่ต้องการ');
        }
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

    const { startDate, endDate } = this.form.getRawValue();
    if (startDate && endDate && new Date(endDate) < new Date(startDate)) {
      this.form.controls.endDate.setErrors({ server: 'วันสิ้นสุดต้องไม่ก่อนวันเริ่ม' });
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.formError.set(null);

    const request = this.buildRequest();
    const call = this.isNew
      ? this.api.createGeneration(this.courseId, request)
      : this.api.updateGeneration(this.generationId!, request);
    call.subscribe({
      next: () => {
        this.toast.success('บันทึกรุ่นอบรมแล้ว');
        this.router.navigateByUrl(`/admin/courses/${this.courseId}`);
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(generation: Generation): void {
    this.form.reset({
      subTitle: generation.subTitle ?? '',
      description: generation.description ?? '',
      startDate: isoToLocalDateTime(generation.startDate),
      endDate: isoToLocalDateTime(generation.endDate),
      quantity: generation.quantity,
      fee: generation.fee,
      photoFileId: generation.photoFileId,
      isCancelled: generation.isCancelled,
    });
  }

  private buildRequest(): SaveGenerationRequest {
    const value = this.form.getRawValue();
    return {
      subTitle: (value.subTitle ?? '').trim() || null,
      description: (value.description ?? '').trim() || null,
      startDate: localDateTimeToIso(value.startDate)!,
      endDate: localDateTimeToIso(value.endDate)!,
      quantity: value.quantity ?? 1,
      fee: value.fee,
      photoFileId: value.photoFileId,
      isCancelled: value.isCancelled ?? false,
    };
  }
}
