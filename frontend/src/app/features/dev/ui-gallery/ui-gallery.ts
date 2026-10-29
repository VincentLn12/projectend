import { JsonPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { applyServerErrors } from '../../../shared/forms/server-errors';
import { urlValidator } from '../../../shared/forms/validators';
import {
  Checkbox,
  DateInput,
  FileUpload,
  SelectInput,
  SelectOption,
  TextArea,
  TextInput,
  Toggle,
} from '../../../shared/ui';

// Development only (see app.routes.ts): every form component in one place, to look at and try out.
@Component({
  selector: 'app-ui-gallery',
  imports: [
    ReactiveFormsModule,
    JsonPipe,
    TextInput,
    TextArea,
    SelectInput,
    Checkbox,
    Toggle,
    DateInput,
    FileUpload,
  ],
  templateUrl: './ui-gallery.html',
})
export class UiGallery {
  protected readonly form = inject(FormBuilder).group({
    title: ['', [Validators.required, Validators.maxLength(50)]],
    email: ['', [Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    year: [null as number | null, [Validators.min(2500), Validators.max(2700)]],
    bio: ['', [Validators.maxLength(200)]],
    kind: [null as number | null, [Validators.required]],
    accepted: [false, [Validators.requiredTrue]],
    published: [true],
    startDate: [null as string | null],
    startAt: [null as string | null],
    link: ['', [urlValidator]],
    cover: [null as string | null],
    gallery: [[] as string[]],
  });

  protected readonly kinds: SelectOption<number>[] = [
    { value: 1, label: 'ข่าวประชาสัมพันธ์' },
    { value: 2, label: 'โครงงาน' },
    { value: 3, label: 'อาจารย์' },
  ];

  protected readonly serverMessage = signal<string | null>(null);

  protected validateAll(): void {
    this.form.markAllAsTouched();
  }

  protected toggleDisabled(): void {
    if (this.form.disabled) {
      this.form.enable();
    } else {
      this.form.disable();
    }
  }

  protected reset(): void {
    this.form.reset({ published: true, accepted: false, gallery: [] });
    this.serverMessage.set(null);
  }

  // What happens when the API answers 400 with field errors.
  protected simulateServerError(): void {
    const response = new HttpErrorResponse({
      status: 400,
      error: {
        errors: { Title: ['ชื่อนี้ถูกใช้แล้ว'], Link: ['ลิงก์นี้เข้าถึงไม่ได้'] },
        detail: 'ข้อมูลไม่ถูกต้อง',
      },
    });
    this.serverMessage.set(applyServerErrors(this.form, response));
  }
}
