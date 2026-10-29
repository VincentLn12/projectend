import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminTechSubjectsApi } from '../api/tech-subjects.api';
import { AdminTechSubject, SaveTechSubjectRequest } from '../types/tech-subject.types';

@Component({
  selector: 'app-tech-subject-edit',
  imports: [ReactiveFormsModule, RouterLink, Button, DataState, FileUpload, TextArea, TextInput, Toggle],
  templateUrl: './tech-subject-edit.html',
})
export class TechSubjectEdit implements OnInit {
  private readonly api = inject(AdminTechSubjectsApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly itemId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.itemId === null;

  protected readonly form = inject(FormBuilder).group({
    icon: ['' as string | null, Validators.maxLength(100)],
    subjectName: ['' as string | null, [Validators.required, Validators.maxLength(200)]],
    subTitle: ['' as string | null, Validators.maxLength(300)],
    description: ['' as string | null],
    sortOrder: [0 as number | null],
    fileIds: [[] as string[]],
    isPublished: [false],
  });

  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    if (this.isNew) {
      return;
    }
    this.loading.set(true);
    this.loadError.set(null);
    this.api.get(this.itemId!).subscribe({
      next: (item) => {
        this.fillForm(item);
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
    const call = this.isNew ? this.api.create(request) : this.api.update(this.itemId!, request);
    call.subscribe({
      next: () => {
        this.toast.success('บันทึกเทคโนโลยีแล้ว');
        this.router.navigateByUrl('/admin/tech-subjects');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(entity: AdminTechSubject): void {
    this.form.reset({
      icon: entity.icon ?? '',
      subjectName: entity.subjectName ?? '',
      subTitle: entity.subTitle ?? '',
      description: entity.description ?? '',
      sortOrder: entity.sortOrder,
      fileIds: entity.imageFileIds,
      isPublished: entity.isPublished,
    });
  }

  private buildRequest(): SaveTechSubjectRequest {
    const value = this.form.getRawValue();
    return {
      icon: this.emptyToNull(value.icon),
      subjectName: (value.subjectName ?? '').trim(),
      subTitle: this.emptyToNull(value.subTitle),
      description: this.emptyToNull(value.description),
      sortOrder: value.sortOrder ?? 0,
      fileIds: value.fileIds ?? [],
      isPublished: value.isPublished ?? false,
    };
  }

  private emptyToNull(text: string | null | undefined): string | null {
    const trimmed = (text ?? '').trim();
    return trimmed === '' ? null : trimmed;
  }
}
