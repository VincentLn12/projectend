import { Component, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { startWith } from 'rxjs';
import { HasUnsavedChanges } from '../../../../shared/forms/unsaved-changes.guard';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { urlValidator } from '../../../../shared/forms/validators';
import { BackLink } from '../../../../shared/navigation/back-link';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminHomeBannersApi } from '../api/home-banners.api';
import { AdminHomeBanner, SaveHomeBannerRequest } from '../types/home-banner.types';

@Component({
  selector: 'app-home-banner-edit',
  imports: [ReactiveFormsModule, RouterLink, BackLink, Button, DataState, FileUpload, TextInput, Toggle],
  templateUrl: './home-banner-edit.html',
})
export class HomeBannerEdit implements OnInit, HasUnsavedChanges {
  private readonly api = inject(AdminHomeBannersApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly itemId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.itemId === null;

  protected readonly form = inject(FormBuilder).group({
    title: ['' as string | null, Validators.maxLength(200)],
    fileId: [null as string | null, Validators.required],
    linkUrl: ['' as string | null, [urlValidator, Validators.maxLength(500)]],
    sortOrder: [0 as number | null],
    isPublished: [false],
  });

  protected readonly formValue = toSignal(
    this.form.valueChanges.pipe(startWith(this.form.getRawValue())),
    { initialValue: this.form.getRawValue() },
  );

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

  hasUnsavedChanges(): boolean {
    return this.form.dirty;
  }

  protected resetForm(): void {
    this.formError.set(null);
    if (this.isNew) {
      this.form.reset({ title: '', fileId: null, linkUrl: '', sortOrder: 0, isPublished: false });
    } else {
      this.load();
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
    const call = this.isNew ? this.api.create(request) : this.api.update(this.itemId!, request);
    call.subscribe({
      next: () => {
        this.form.markAsPristine();
        this.toast.success('บันทึกแบนเนอร์แล้ว');
        this.router.navigateByUrl('/admin/home-banners');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(entity: AdminHomeBanner): void {
    this.form.reset({
      title: entity.title ?? '',
      fileId: entity.fileId,
      linkUrl: entity.linkUrl ?? '',
      sortOrder: entity.sortOrder,
      isPublished: entity.isPublished,
    });
  }

  private buildRequest(): SaveHomeBannerRequest {
    const value = this.form.getRawValue();
    return {
      title: this.emptyToNull(value.title),
      fileId: value.fileId!,
      linkUrl: this.emptyToNull(value.linkUrl),
      sortOrder: value.sortOrder ?? 0,
      isPublished: value.isPublished ?? false,
    };
  }

  private emptyToNull(text: string | null | undefined): string | null {
    const trimmed = (text ?? '').trim();
    return trimmed === '' ? null : trimmed;
  }
}
