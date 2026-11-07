import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { thaiPhoneValidator } from '../../../../shared/forms/validators';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { ShortCoursesApi } from '../api/short-courses.api';
import {
  AVAILABILITY_BADGE_CLASSES,
  AVAILABILITY_FULL,
  AVAILABILITY_LABELS,
  AVAILABILITY_NOT_OPEN_YET,
  AVAILABILITY_OPEN,
  MyShortCourseRegistration,
  ShortCourseDetail,
} from '../types/short-course.types';

const PREFIXES = ['นาย', 'นาง', 'นางสาว', 'เด็กชาย', 'เด็กหญิง'];

@Component({
  selector: 'app-public-short-course-detail',
  imports: [RouterLink, ReactiveFormsModule, ThaiDatePipe, Button, DataState, FileImage, SelectInput, TextInput],
  templateUrl: './short-course-detail.html',
})
export class PublicShortCourseDetail implements OnInit {
  private readonly api = inject(ShortCoursesApi);
  private readonly toast = inject(ToastService);
  private readonly courseId = inject(ActivatedRoute).snapshot.paramMap.get('id')!;

  protected readonly course = signal<ShortCourseDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly registration = signal<MyShortCourseRegistration | null>(null);

  protected readonly availabilityLabels = AVAILABILITY_LABELS;
  protected readonly availabilityBadgeClasses = AVAILABILITY_BADGE_CLASSES;
  protected readonly isOpen = computed(() => this.course()?.availability === AVAILABILITY_OPEN);
  protected readonly closedMessage = computed(() => {
    const availability = this.course()?.availability;
    if (availability === AVAILABILITY_NOT_OPEN_YET) {
      return 'หลักสูตรนี้ยังไม่เปิดรับสมัคร';
    }
    if (availability === AVAILABILITY_FULL) {
      return 'ขออภัย จำนวนผู้สมัครเต็มแล้ว';
    }
    return 'หลักสูตรนี้ปิดรับสมัครแล้ว';
  });

  protected readonly prefixOptions: SelectOption<string>[] = PREFIXES.map((prefix) => ({ value: prefix, label: prefix }));

  protected readonly form = inject(FormBuilder).group({
    prefix: [null as string | null],
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    phone: ['', [Validators.required, thaiPhoneValidator]],
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get(this.courseId).subscribe({
      next: (course) => {
        this.course.set(course);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }

  protected seatsText(course: ShortCourseDetail): string {
    if (course.maxParticipants === 0) {
      return `สมัครแล้ว ${course.registeredCount} คน (ไม่จำกัดจำนวน)`;
    }
    const left = Math.max(course.maxParticipants - course.registeredCount, 0);
    return `เหลือ ${left} จาก ${course.maxParticipants} ที่นั่ง`;
  }

  protected timeText(iso: string): string {
    return new Date(iso).toLocaleTimeString('th-TH', { hour: '2-digit', minute: '2-digit' });
  }

  protected sameDay(course: ShortCourseDetail): boolean {
    return new Date(course.startDate).toDateString() === new Date(course.endDate).toDateString();
  }

  protected submit(): void {
    if (this.saving()) {
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
      .register(this.courseId, {
        prefix: value.prefix,
        firstName: (value.firstName ?? '').trim(),
        lastName: (value.lastName ?? '').trim(),
        phone: (value.phone ?? '').trim(),
      })
      .subscribe({
        next: (registration) => {
          this.saving.set(false);
          this.registration.set(registration);
          this.toast.success('สมัครอบรมเรียบร้อยแล้ว');
          this.load();
        },
        error: (error: unknown) => {
          this.saving.set(false);
          this.formError.set(applyServerErrors(this.form, error));
          this.load();
        },
      });
  }
}
