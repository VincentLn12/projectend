import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { BackLink } from '../../../../shared/navigation/back-link';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminCertificateTemplatesApi } from '../api/certificate-templates.api';
import { CertificateTemplate, SaveCertificateTemplateRequest } from '../types/certificate-template.types';

@Component({
  selector: 'app-certificate-template-edit',
  imports: [ReactiveFormsModule, BackLink, Button, DataState, FileUpload, TextInput],
  templateUrl: './certificate-template-edit.html',
})
export class CertificateTemplateEdit implements OnInit {
  private readonly api = inject(AdminCertificateTemplatesApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly templateId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.templateId === null;

  protected readonly form = inject(FormBuilder).group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    signerName: ['', [Validators.required, Validators.maxLength(200)]],
    signerPosition: ['', [Validators.required, Validators.maxLength(300)]],
    secondSignerName: ['', Validators.maxLength(200)],
    secondSignerPosition: ['', Validators.maxLength(300)],
    backgroundFileId: [null as string | null],
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
    this.api.get(this.templateId!).subscribe({
      next: (template) => {
        this.fillForm(template);
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
    const call = this.isNew ? this.api.create(request) : this.api.update(this.templateId!, request);
    call.subscribe({
      next: () => {
        this.toast.success('บันทึกแม่แบบแล้ว');
        this.router.navigateByUrl('/admin/certificate-templates');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(template: CertificateTemplate): void {
    this.form.reset({
      name: template.name,
      signerName: template.signerName,
      signerPosition: template.signerPosition,
      secondSignerName: template.secondSignerName ?? '',
      secondSignerPosition: template.secondSignerPosition ?? '',
      backgroundFileId: template.backgroundFileId,
    });
  }

  private buildRequest(): SaveCertificateTemplateRequest {
    const value = this.form.getRawValue();
    return {
      name: (value.name ?? '').trim(),
      signerName: (value.signerName ?? '').trim(),
      signerPosition: (value.signerPosition ?? '').trim(),
      secondSignerName: (value.secondSignerName ?? '').trim() || null,
      secondSignerPosition: (value.secondSignerPosition ?? '').trim() || null,
      backgroundFileId: value.backgroundFileId,
    };
  }
}
