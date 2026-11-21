import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { startWith } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { applyServerErrors, describeHttpError, describeProblemDetail } from '../../../shared/forms/server-errors';
import { thaiPhoneValidator, urlValidator } from '../../../shared/forms/validators';
import { ThaiDatePipe } from '../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../shared/ui/button/button';
import { ConfirmService } from '../../../shared/ui/confirm-dialog/confirm.service';
import { DataState } from '../../../shared/ui/data-state/data-state';
import { DateInput } from '../../../shared/ui/date-input/date-input';
import { FileUpload } from '../../../shared/ui/file-upload/file-upload';
import { TextArea } from '../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../shared/ui/toggle/toggle';
import { ProfileApi } from '../api/profile.api';
import { MyJob, MyProfile } from '../types/profile.types';

const BIO_MAX_LENGTH = 1000;

@Component({
  selector: 'app-my-profile',
  imports: [ReactiveFormsModule, RouterLink, ThaiDatePipe, Button, DataState, DateInput, FileUpload, TextArea, TextInput, Toggle],
  templateUrl: './my-profile.html',
})
export class MyProfilePage implements OnInit {
  private readonly api = inject(ProfileApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);
  private readonly formBuilder = inject(FormBuilder);
  protected readonly auth = inject(AuthService);

  protected readonly bioMaxLength = BIO_MAX_LENGTH;

  protected readonly profile = signal<MyProfile | null>(null);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly form = this.formBuilder.group({
    imageFileId: [null as string | null],
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    bio: ['', Validators.maxLength(BIO_MAX_LENGTH)],
    skills: ['', Validators.maxLength(500)],
    showEmail: [false],
    phone: ['', thaiPhoneValidator],
    showPhone: [false],
    linkedInUrl: ['', [Validators.maxLength(500), urlValidator]],
    githubUrl: ['', [Validators.maxLength(500), urlValidator]],
    showProfile: [false],
    previousSchool: ['', Validators.maxLength(200)],
    address: ['', Validators.maxLength(500)],
  });

  private readonly formValue = toSignal(this.form.valueChanges.pipe(startWith(this.form.getRawValue())), {
    initialValue: this.form.getRawValue(),
  });

  protected readonly bioLength = computed(() => (this.formValue().bio ?? '').length);
  protected readonly skillTags = computed(() =>
    (this.formValue().skills ?? '')
      .split(',')
      .map((word) => word.trim())
      .filter((word) => word.length > 0),
  );

  protected readonly checklist = computed(() => {
    const value = this.formValue();
    return [
      { label: 'รูปโปรไฟล์', done: !!value.imageFileId },
      { label: 'เกี่ยวกับฉัน', done: !!value.bio?.trim() },
      { label: 'ทักษะ', done: !!value.skills?.trim() },
      { label: 'ช่องทางติดต่อ', done: !!(value.showEmail || (value.showPhone && value.phone) || value.linkedInUrl || value.githubUrl) },
      { label: 'เปิดหน้า portfolio', done: !!value.showProfile },
    ];
  });

  protected readonly doneCount = computed(() => this.checklist().filter((item) => item.done).length);

  protected readonly passwordForm = this.formBuilder.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128)]],
    confirmPassword: ['', Validators.required],
  });
  protected readonly changingPassword = signal(false);
  protected readonly passwordError = signal<string | null>(null);

  protected readonly jobs = signal<MyJob[]>([]);
  protected readonly editingJobId = signal<string | 'new' | null>(null);
  protected readonly savingJob = signal(false);
  protected readonly jobError = signal<string | null>(null);
  protected readonly jobForm = this.formBuilder.group({
    jobName: ['', [Validators.required, Validators.maxLength(200)]],
    company: ['', Validators.maxLength(200)],
    position: ['', Validators.maxLength(200)],
    startDate: ['' as string | null, Validators.required],
    description: ['', Validators.maxLength(1000)],
    isPublished: [true],
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.get().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.fillForm(profile);
        this.loading.set(false);
        if (this.auth.isStudent()) {
          this.api.jobs().subscribe({ next: (jobs) => this.jobs.set(jobs) });
        }
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.loadError.set(describeHttpError(error));
      },
    });
  }

  protected save(): void {
    const current = this.profile();
    if (!current || this.saving()) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.formError.set(null);

    const value = this.form.getRawValue();
    this.api
      .save({
        fullName: (value.fullName ?? '').trim(),
        bio: (value.bio ?? '').trim() || null,
        imageFileId: value.imageFileId,
        showEmail: value.showEmail ?? false,
        showProfile: value.showProfile ?? false,
        phone: (value.phone ?? '').trim() || null,
        showPhone: value.showPhone ?? false,
        linkedInUrl: (value.linkedInUrl ?? '').trim() || null,
        githubUrl: (value.githubUrl ?? '').trim() || null,
        skills: (value.skills ?? '').trim() || null,
        previousSchool: (value.previousSchool ?? '').trim() || null,
        address: (value.address ?? '').trim() || null,
      })
      .subscribe({
        next: (updated) => {
          this.saving.set(false);
          this.profile.set(updated);
          this.toast.success('บันทึกโปรไฟล์แล้ว');
        },
        error: (error: unknown) => {
          this.saving.set(false);
          this.formError.set(applyServerErrors(this.form, error));
        },
      });
  }

  protected changePassword(): void {
    if (this.changingPassword()) {
      return;
    }

    const value = this.passwordForm.getRawValue();
    if (value.newPassword !== value.confirmPassword) {
      this.passwordForm.controls.confirmPassword.setErrors({ server: 'รหัสผ่านใหม่ทั้งสองช่องไม่ตรงกัน' });
    }
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.changingPassword.set(true);
    this.passwordError.set(null);

    this.api.changePassword(value.currentPassword ?? '', value.newPassword ?? '').subscribe({
      next: () => {
        this.changingPassword.set(false);
        this.toast.success('เปลี่ยนรหัสผ่านแล้ว กรุณาเข้าสู่ระบบใหม่');
        this.auth.logout();
      },
      error: (error: unknown) => {
        this.changingPassword.set(false);
        const message = describeProblemDetail(error);
        this.passwordError.set(message.includes('Incorrect password') ? 'รหัสผ่านปัจจุบันไม่ถูกต้อง' : message);
      },
    });
  }

  protected startJob(job: MyJob | null): void {
    this.jobError.set(null);
    this.jobForm.reset({
      jobName: job?.jobName ?? '',
      company: job?.company ?? '',
      position: job?.position ?? '',
      startDate: job?.startDate ?? '',
      description: job?.description ?? '',
      isPublished: job?.isPublished ?? true,
    });
    this.editingJobId.set(job ? job.id : 'new');
  }

  protected cancelJob(): void {
    this.editingJobId.set(null);
  }

  protected saveJob(): void {
    const editingId = this.editingJobId();
    if (!editingId || this.savingJob()) {
      return;
    }
    if (this.jobForm.invalid) {
      this.jobForm.markAllAsTouched();
      return;
    }

    this.savingJob.set(true);
    this.jobError.set(null);

    const value = this.jobForm.getRawValue();
    const request = {
      jobName: (value.jobName ?? '').trim(),
      company: (value.company ?? '').trim() || null,
      position: (value.position ?? '').trim() || null,
      startDate: value.startDate!,
      description: (value.description ?? '').trim() || null,
      isPublished: value.isPublished ?? false,
    };
    const call = editingId === 'new' ? this.api.createJob(request) : this.api.updateJob(editingId, request);
    call.subscribe({
      next: () => {
        this.savingJob.set(false);
        this.editingJobId.set(null);
        this.toast.success('บันทึกประสบการณ์ทำงานแล้ว');
        this.api.jobs().subscribe({ next: (jobs) => this.jobs.set(jobs) });
      },
      error: (error: unknown) => {
        this.savingJob.set(false);
        this.jobError.set(applyServerErrors(this.jobForm, error));
      },
    });
  }

  protected async deleteJob(job: MyJob): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบประสบการณ์ทำงานนี้?',
      message: `${job.jobName}${job.company ? ' · ' + job.company : ''}`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.deleteJob(job.id).subscribe({
      next: () => {
        this.toast.success('ลบรายการแล้ว');
        this.jobs.update((list) => list.filter((item) => item.id !== job.id));
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  private fillForm(profile: MyProfile): void {
    this.form.reset({
      imageFileId: profile.imageFileId,
      fullName: profile.fullName,
      bio: profile.bio ?? '',
      skills: profile.skills ?? '',
      showEmail: profile.showEmail,
      phone: profile.phone ?? '',
      showPhone: profile.showPhone,
      linkedInUrl: profile.linkedInUrl ?? '',
      githubUrl: profile.githubUrl ?? '',
      showProfile: profile.showProfile,
      previousSchool: profile.student?.previousSchool ?? '',
      address: profile.student?.address ?? '',
    });
  }
}
