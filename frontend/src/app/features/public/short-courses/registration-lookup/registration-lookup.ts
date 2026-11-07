import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
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
export class RegistrationLookup implements OnInit {
  private readonly api = inject(ShortCoursesApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly lookupForm = this.formBuilder.group({
    code: ['', [Validators.required, Validators.maxLength(20)]],
    phone: ['', [Validators.required, thaiPhoneValidator]],
  });

  protected readonly editForm = this.formBuilder.group({
    prefix: [null as string | null],
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
  });

  protected readonly prefixOptions: SelectOption<string>[] = PREFIXES.map((prefix) => ({ value: prefix, label: prefix }));

  protected readonly registration = signal<MyShortCourseRegistration | null>(null);
  protected readonly searching = signal(false);
  protected readonly lookupError = signal<string | null>(null);
  protected readonly editing = signal(false);
  protected readonly saving = signal(false);
  protected readonly editError = signal<string | null>(null);
  protected readonly downloading = signal(false);

  private verifiedPhone = '';

  ngOnInit(): void {
    const code = this.route.snapshot.queryParamMap.get('code');
    if (code) {
      this.lookupForm.patchValue({ code });
    }
  }

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

    const value = this.lookupForm.getRawValue();
    const phone = (value.phone ?? '').trim();
    this.api.lookup({ code: (value.code ?? '').trim(), phone }).subscribe({
      next: (registration) => {
        this.searching.set(false);
        this.verifiedPhone = phone;
        this.show(registration);
      },
      error: (error: unknown) => {
        this.searching.set(false);
        this.registration.set(null);
        this.lookupError.set(describeProblemDetail(error));
      },
    });
  }

  protected startEditing(): void {
    this.editError.set(null);
    this.editing.set(true);
  }

  protected cancelEditing(): void {
    this.editing.set(false);
  }

  protected saveEdit(): void {
    const current = this.registration();
    if (!current || this.saving()) {
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
      .update(current.code, {
        currentPhone: this.verifiedPhone,
        prefix: value.prefix,
        firstName: (value.firstName ?? '').trim(),
        lastName: (value.lastName ?? '').trim(),
      })
      .subscribe({
        next: (updated) => {
          this.saving.set(false);
          this.editing.set(false);
          this.show(updated);
          this.toast.success('แก้ไขข้อมูลแล้ว');
        },
        error: (error: unknown) => {
          this.saving.set(false);
          this.editError.set(applyServerErrors(this.editForm, error));
        },
      });
  }

  protected async cancelRegistration(): Promise<void> {
    const current = this.registration();
    if (!current) {
      return;
    }

    const confirmed = await this.confirm.ask({
      title: 'ยกเลิกการสมัครอบรมนี้?',
      message: `${current.courseTitle} (${current.code}) ที่นั่งจะถูกคืนให้ผู้อื่น`,
      confirmText: 'ยกเลิกการสมัคร',
      cancelText: 'ไม่ยกเลิก',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.cancel(current.code, this.verifiedPhone).subscribe({
      next: () => {
        this.toast.success('ยกเลิกการสมัครแล้ว');
        this.registration.set({ ...current, status: REGISTRATION_CANCELLED, canModify: false });
        this.editing.set(false);
      },
      error: (error: unknown) => this.toast.error(describeProblemDetail(error)),
    });
  }

  protected downloadCertificate(): void {
    const current = this.registration();
    if (!current || this.downloading()) {
      return;
    }

    this.downloading.set(true);
    this.api.certificate({ code: current.code, phone: this.verifiedPhone }).subscribe({
      next: (blob) => {
        saveBlobAsFile(blob, `เกียรติบัตร-${current.code}.pdf`);
        this.downloading.set(false);
      },
      error: async (error: unknown) => {
        this.downloading.set(false);
        this.toast.error(await describeDownloadError(error));
      },
    });
  }

  private show(registration: MyShortCourseRegistration): void {
    this.registration.set(registration);
    this.editForm.reset({
      prefix: registration.prefix,
      firstName: registration.firstName,
      lastName: registration.lastName,
    });
  }
}
