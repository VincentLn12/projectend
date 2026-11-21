import { Component, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { startWith } from 'rxjs';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { BackLink } from '../../../../shared/navigation/back-link';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { AdminLecturersApi } from '../api/lecturers.api';
import { AdminLecturer, SaveLecturerRequest } from '../types/lecturer.types';

@Component({
  selector: 'app-lecturer-edit',
  imports: [ReactiveFormsModule, RouterLink, ThaiDatePipe, BackLink, Button, DataState, FileUpload, TextArea, TextInput, Toggle],
  templateUrl: './lecturer-edit.html',
})
export class LecturerEdit implements OnInit {
  private readonly api = inject(AdminLecturersApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly lecturerId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.lecturerId === null;

  protected readonly form = inject(FormBuilder).group({
    prefix: ['', Validators.maxLength(50)],
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    position: ['', Validators.maxLength(200)],
    expertise: ['', Validators.maxLength(500)],
    contact: ['', Validators.maxLength(300)],
    imageFileId: [null as string | null],
    sortOrder: [0 as number | null, [Validators.required, Validators.min(0)]],
    isPublished: [false],
  });

  protected readonly loadedLecturer = signal<AdminLecturer | null>(null);

  protected readonly formValue = toSignal(
    this.form.valueChanges.pipe(startWith(this.form.getRawValue())),
    { initialValue: this.form.getRawValue() },
  );

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
    this.api.get(this.lecturerId!).subscribe({
      next: (lecturer) => {
        this.loadedLecturer.set(lecturer);
        this.fillForm(lecturer);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.loadError.set(describeHttpError(error));
      },
    });
  }

  protected resetForm(): void {
    this.formError.set(null);
    const existing = this.loadedLecturer();
    if (existing) {
      this.fillForm(existing);
    } else {
      this.form.reset({
        prefix: '',
        fullName: '',
        position: '',
        expertise: '',
        contact: '',
        imageFileId: null,
        sortOrder: 0,
        isPublished: false,
      });
    }
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
    const call = this.isNew ? this.api.create(request) : this.api.update(this.lecturerId!, request);
    call.subscribe({
      next: () => {
        this.toast.success('บันทึกข้อมูลอาจารย์แล้ว');
        this.router.navigateByUrl('/admin/lecturers');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(lecturer: AdminLecturer): void {
    this.form.reset({
      prefix: lecturer.prefix ?? '',
      fullName: lecturer.fullName,
      position: lecturer.position ?? '',
      expertise: lecturer.expertise ?? '',
      contact: lecturer.contact ?? '',
      imageFileId: lecturer.imageFileId,
      sortOrder: lecturer.sortOrder,
      isPublished: lecturer.isPublished,
    });
  }

  private buildRequest(): SaveLecturerRequest {
    const value = this.form.getRawValue();
    return {
      prefix: this.emptyToNull(value.prefix),
      fullName: (value.fullName ?? '').trim(),
      position: this.emptyToNull(value.position),
      expertise: this.emptyToNull(value.expertise),
      contact: this.emptyToNull(value.contact),
      imageFileId: value.imageFileId,
      sortOrder: value.sortOrder ?? 0,
      isPublished: value.isPublished ?? false,
    };
  }

  private emptyToNull(text: string | null | undefined): string | null {
    const trimmed = (text ?? '').trim();
    return trimmed === '' ? null : trimmed;
  }
}
