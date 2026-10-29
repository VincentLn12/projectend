import { Component, computed, effect, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { CoursesApi, describeRegistrationError } from '../api/courses.api';
import { CourseDetail, Generation, MyRegistration, REGISTRATION_CANCELLED } from '../types/course.types';

@Component({
  selector: 'app-public-course-detail',
  imports: [RouterLink, ThaiDatePipe, Button, DataState, FileImage],
  templateUrl: './course-detail.html',
})
export class PublicCourseDetail implements OnInit {
  private readonly api = inject(CoursesApi);
  private readonly router = inject(Router);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);
  private readonly courseId = inject(ActivatedRoute).snapshot.paramMap.get('id')!;
  protected readonly auth = inject(AuthService);

  protected readonly course = signal<CourseDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly busyGenerationId = signal<string | null>(null);

  private readonly myRegistrations = signal<MyRegistration[]>([]);
  protected readonly registeredIds = computed(
    () =>
      new Set(
        this.myRegistrations()
          .filter((registration) => registration.status !== REGISTRATION_CANCELLED)
          .map((registration) => registration.generationId),
      ),
  );

  constructor() {
    // Knowing which generations the signed-in person already joined; empty for visitors.
    effect(() => {
      if (this.auth.isLoggedIn()) {
        this.loadMyRegistrations();
      } else {
        this.myRegistrations.set([]);
      }
    });
  }

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

  protected seatsLeft(generation: Generation): number {
    return Math.max(generation.quantity - generation.registeredCount, 0);
  }

  protected goToLogin(): void {
    this.router.navigate(['/login'], { queryParams: { returnUrl: `/courses/${this.courseId}` } });
  }

  protected register(generation: Generation): void {
    this.busyGenerationId.set(generation.id);
    this.api.register(generation.id).subscribe({
      next: () => {
        this.toast.success('ลงทะเบียนเรียบร้อยแล้ว');
        this.refreshAfterChange();
      },
      error: (error: unknown) => {
        this.toast.error(describeRegistrationError(error));
        this.refreshAfterChange();
      },
    });
  }

  protected async cancel(generation: Generation): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ยกเลิกการลงทะเบียน?',
      message: 'ที่นั่งของคุณจะถูกคืนให้ผู้อื่น',
      confirmText: 'ยกเลิกการลงทะเบียน',
      cancelText: 'ไม่ยกเลิก',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.busyGenerationId.set(generation.id);
    this.api.cancelRegistration(generation.id).subscribe({
      next: () => {
        this.toast.success('ยกเลิกการลงทะเบียนแล้ว');
        this.refreshAfterChange();
      },
      error: (error: unknown) => {
        this.toast.error(describeRegistrationError(error));
        this.refreshAfterChange();
      },
    });
  }

  // The seat counts and the person's own list may both have changed.
  private refreshAfterChange(): void {
    this.busyGenerationId.set(null);
    this.loadMyRegistrations();
    this.api.get(this.courseId).subscribe({ next: (course) => this.course.set(course), error: () => undefined });
  }

  private loadMyRegistrations(): void {
    this.api.myRegistrations().subscribe({
      next: (registrations) => this.myRegistrations.set(registrations),
      error: () => undefined,
    });
  }
}
