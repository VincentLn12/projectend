import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { describeDownloadError, saveBlobAsFile } from '../../../../shared/data/download-file';
import { applyServerErrors, describeProblemDetail } from '../../../../shared/forms/server-errors';
import { thaiPhoneValidator } from '../../../../shared/forms/validators';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { ShortCoursesApi } from '../api/short-courses.api';
import { MyShortCourseRegistration, REGISTRATION_CANCELLED } from '../types/short-course.types';

const PREFIXES = ['นาย', 'นาง', 'นางสาว', 'เด็กชาย', 'เด็กหญิง'];

@Component({
  selector: 'app-registration-lookup',
  imports: [RouterLink, ReactiveFormsModule, ThaiDatePipe, Button, SelectInput, TextInput],
  templateUrl: './registration-lookup.html',
})
export class RegistrationLookup {
  private readonly api = inject(ShortCoursesApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly lookupForm = this.formBuilder.group({
    phone: ['', [Validators.required, thaiPhoneValidator]],
  });

  protected readonly editForm = this.formBuilder.group({
    prefix: [null as string | null],
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
  });

  protected readonly prefixOptions: SelectOption<string>[] = PREFIXES.map((prefix) => ({ value: prefix, label: prefix }));

  protected readonly registrations = signal<MyShortCourseRegistration[] | null>(null);
  protected readonly searching = signal(false);
  protected readonly lookupError = signal<string | null>(null);
  protected readonly editingId = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly editError = signal<string | null>(null);
  protected readonly downloadingId = signal<string | null>(null);

  private verifiedPhone = '';

  protected isCancelled(item: MyShortCourseRegistration): boolean {
    return item.status === REGISTRATION_CANCELLED;
  }

  protected search(): void {
    if (this.searching()) {
      return;
    }
    if (this.lookupForm.invalid) {
      this.lookupForm.markAllAsTouched();
      return;
    }

    this.searching.set(true);
    this.lookupError.set(null);
    this.editingId.set(null);

    const phone = (this.lookupForm.getRawValue().phone ?? '').trim();
    this.api.lookup({ phone }).subscribe({
      next: (registrations) => {
        this.searching.set(false);
        this.verifiedPhone = phone;
        this.registrations.set(registrations);
      },
      error: (error: unknown) => {
        this.searching.set(false);
        this.registrations.set(null);
        this.lookupError.set(describeProblemDetail(error));
      },
    });
  }

  protected startEditing(item: MyShortCourseRegistration): void {
    this.editError.set(null);
    this.editForm.reset({ prefix: item.prefix, firstName: item.firstName, lastName: item.lastName });
    this.editingId.set(item.id);
  }

  protected cancelEditing(): void {
    this.editingId.set(null);
  }

  protected saveEdit(item: MyShortCourseRegistration): void {
    if (this.saving()) {
      return;
    }
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.editError.set(null);

    const value = this.editForm.getRawValue();
    this.api
      .update(item.id, {
        currentPhone: this.verifiedPhone,
        prefix: value.prefix,
        firstName: (value.firstName ?? '').trim(),
        lastName: (value.lastName ?? '').trim(),
      })
      .subscribe({
        next: (updated) => {
          this.saving.set(false);
          this.editingId.set(null);
          this.replace(updated);
          this.toast.success('แก้ไขข้อมูลแล้ว');
        },
        error: (error: unknown) => {
          this.saving.set(false);
          this.editError.set(applyServerErrors(this.editForm, error));
        },
      });
  }

  protected async cancelRegistration(item: MyShortCourseRegistration): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ยกเลิกการสมัครอบรมนี้?',
      message: `${item.courseTitle} ที่นั่งจะถูกคืนให้ผู้อื่น`,
      confirmText: 'ยกเลิกการสมัคร',
      cancelText: 'ไม่ยกเลิก',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.cancel(item.id, this.verifiedPhone).subscribe({
      next: () => {
        this.toast.success('ยกเลิกการสมัครแล้ว');
        this.editingId.set(null);
        this.replace({ ...item, status: REGISTRATION_CANCELLED, canModify: false });
      },
      error: (error: unknown) => this.toast.error(describeProblemDetail(error)),
    });
  }

  protected downloadCertificate(item: MyShortCourseRegistration): void {
    if (this.downloadingId()) {
      return;
    }

    this.downloadingId.set(item.id);
    this.api.certificate({ registrationId: item.id, phone: this.verifiedPhone }).subscribe({
      next: (blob) => {
        saveBlobAsFile(blob, `เกียรติบัตร-${item.firstName}.pdf`);
        this.downloadingId.set(null);
      },
      error: async (error: unknown) => {
        this.downloadingId.set(null);
        this.toast.error(await describeDownloadError(error));
      },
    });
  }

  private replace(updated: MyShortCourseRegistration): void {
    this.registrations.update((list) => (list ?? []).map((item) => (item.id === updated.id ? updated : item)));
  }
}
