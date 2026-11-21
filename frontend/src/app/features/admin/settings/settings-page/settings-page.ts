import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { urlValidator } from '../../../../shared/forms/validators';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminSettingsApi } from '../api/settings.api';
import { SiteSettings } from '../types/settings.types';

@Component({
  selector: 'app-settings-page',
  imports: [ReactiveFormsModule, Button, DataState, FileUpload, TextInput],
  templateUrl: './settings-page.html',
})
export class SettingsPage implements OnInit {
  private readonly api = inject(AdminSettingsApi);
  private readonly toast = inject(ToastService);

  protected readonly form = inject(FormBuilder).group({
    webName: ['', [Validators.required, Validators.maxLength(200)]],
    universityName: ['', Validators.maxLength(200)],
    footerText: ['', Validators.maxLength(500)],
    registerUrl: ['', [urlValidator, Validators.maxLength(500)]],
    kruUrl: ['', [urlValidator, Validators.maxLength(500)]],
    pageFacebook: ['', [urlValidator, Validators.maxLength(500)]],
    logoFileId: [null as string | null],
    videoUrl: ['', Validators.maxLength(500)],
    mapLocation: ['', Validators.maxLength(500)],
  });

  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.get().subscribe({
      next: (settings) => {
        this.fillForm(settings);
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

    this.api.save(this.buildRequest()).subscribe({
      next: (saved) => {
        this.saving.set(false);
        this.fillForm(saved);
        this.toast.success('บันทึกการตั้งค่าแล้ว');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(settings: SiteSettings): void {
    this.form.reset({
      webName: settings.webName,
      universityName: settings.universityName ?? '',
      footerText: settings.footerText ?? '',
      registerUrl: settings.registerUrl ?? '',
      kruUrl: settings.kruUrl ?? '',
      pageFacebook: settings.pageFacebook ?? '',
      logoFileId: settings.logoFileId,
      videoUrl: settings.videoUrl ?? '',
      mapLocation: settings.mapLocation ?? '',
    });
  }

  private buildRequest(): SiteSettings {
    const value = this.form.getRawValue();
    return {
      webName: (value.webName ?? '').trim(),
      universityName: this.emptyToNull(value.universityName),
      footerText: this.emptyToNull(value.footerText),
      registerUrl: this.emptyToNull(value.registerUrl),
      kruUrl: this.emptyToNull(value.kruUrl),
      pageFacebook: this.emptyToNull(value.pageFacebook),
      logoFileId: value.logoFileId,
      videoUrl: this.emptyToNull(value.videoUrl),
      mapLocation: this.emptyToNull(value.mapLocation),
    };
  }

  private emptyToNull(text: string | null | undefined): string | null {
    const trimmed = (text ?? '').trim();
    return trimmed === '' ? null : trimmed;
  }
}
