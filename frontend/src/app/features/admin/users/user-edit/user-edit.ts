import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminUsersApi, describeUserError } from '../api/users.api';
import {
  AdminUser,
  CreateUserRequest,
  UpdateUserRequest,
  USER_TYPE_GENERAL,
  USER_TYPE_LABELS,
  USER_TYPE_STUDENT,
} from '../types/user.types';

const MIN_PASSWORD_LENGTH = 8;

@Component({
  selector: 'app-user-edit',
  imports: [ReactiveFormsModule, RouterLink, Button, DataState, SelectInput, TextArea, TextInput],
  templateUrl: './user-edit.html',
})
export class UserEdit implements OnInit {
  private readonly api = inject(AdminUsersApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly userId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.userId === null;

  protected readonly typeOptions: SelectOption<number>[] = Object.entries(USER_TYPE_LABELS).map(([value, label]) => ({
    value: Number(value),
    label,
  }));

  protected readonly form = inject(FormBuilder).group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    password: ['', [Validators.required, Validators.minLength(MIN_PASSWORD_LENGTH), Validators.maxLength(128)]],
    userType: [USER_TYPE_GENERAL as number | null, Validators.required],
    studentCode: ['', Validators.maxLength(20)],
    entryYear: [null as number | null, [Validators.min(1900), Validators.max(2700)]],
    previousSchool: ['', Validators.maxLength(200)],
    address: ['', Validators.maxLength(500)],
  });

  protected readonly passwordForm = inject(FormBuilder).group({
    newPassword: ['', [Validators.required, Validators.minLength(MIN_PASSWORD_LENGTH), Validators.maxLength(128)]],
  });

  protected readonly user = signal<AdminUser | null>(null);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly resetting = signal(false);
  protected readonly formError = signal<string | null>(null);

  private readonly selectedType = signal<number | null>(USER_TYPE_GENERAL);
  protected readonly isStudent = computed(() => this.selectedType() === USER_TYPE_STUDENT);

  constructor() {
    this.form.controls.userType.valueChanges.subscribe((type) => this.selectedType.set(type));
    if (!this.isNew) {
      // Email and password cannot be changed here: the password has its own "reset" box.
      this.form.controls.email.disable();
      this.form.controls.password.disable();
    }
  }

  ngOnInit(): void {
    if (!this.isNew) {
      this.load();
    }
  }

  protected load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.get(this.userId!).subscribe({
      next: (user) => {
        this.user.set(user);
        this.fillForm(user);
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
    if (this.isStudent()) {
      const { studentCode, entryYear } = this.form.getRawValue();
      if (!studentCode?.trim()) {
        this.form.controls.studentCode.setErrors({ server: 'นักศึกษาต้องระบุรหัสนักศึกษา' });
      }
      if (!entryYear) {
        this.form.controls.entryYear.setErrors({ server: 'นักศึกษาต้องระบุปีที่เข้าศึกษา' });
      }
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.formError.set(null);

    const call = this.isNew ? this.api.create(this.buildCreateRequest()) : this.api.update(this.userId!, this.buildUpdateRequest());
    call.subscribe({
      next: () => {
        this.toast.success('บันทึกผู้ใช้แล้ว');
        this.router.navigateByUrl('/admin/users');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(describeUserError(error));
      },
    });
  }

  protected resetPassword(): void {
    if (this.resetting()) {
      return;
    }
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.resetting.set(true);
    this.api.resetPassword(this.userId!, this.passwordForm.controls.newPassword.value ?? '').subscribe({
      next: () => {
        this.resetting.set(false);
        this.passwordForm.reset({ newPassword: '' });
        this.toast.success('ตั้งรหัสผ่านใหม่แล้ว ผู้ใช้ต้องเข้าสู่ระบบใหม่');
      },
      error: (error: unknown) => {
        this.resetting.set(false);
        this.toast.error(describeUserError(error));
      },
    });
  }

  private fillForm(user: AdminUser): void {
    this.form.reset({
      email: user.email,
      fullName: user.fullName,
      password: '',
      userType: user.userType,
      studentCode: user.student?.studentCode ?? '',
      entryYear: user.student?.entryYear ?? null,
      previousSchool: user.student?.previousSchool ?? '',
      address: user.student?.address ?? '',
    });
    this.selectedType.set(user.userType);
  }

  private buildCreateRequest(): CreateUserRequest {
    const value = this.form.getRawValue();
    return {
      email: (value.email ?? '').trim(),
      password: value.password ?? '',
      ...this.buildProfile(),
    };
  }

  private buildUpdateRequest(): UpdateUserRequest {
    return this.buildProfile();
  }

  private buildProfile(): UpdateUserRequest {
    const value = this.form.getRawValue();
    const student = value.userType === USER_TYPE_STUDENT;
    return {
      fullName: (value.fullName ?? '').trim(),
      userType: value.userType ?? USER_TYPE_GENERAL,
      studentCode: student ? this.emptyToNull(value.studentCode) : null,
      entryYear: student ? value.entryYear : null,
      previousSchool: student ? this.emptyToNull(value.previousSchool) : null,
      address: student ? this.emptyToNull(value.address) : null,
    };
  }

  private emptyToNull(text: string | null | undefined): string | null {
    const trimmed = (text ?? '').trim();
    return trimmed === '' ? null : trimmed;
  }
}
