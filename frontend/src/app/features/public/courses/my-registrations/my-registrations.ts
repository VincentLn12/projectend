import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { CoursesApi, describeRegistrationError } from '../api/courses.api';
import {
  MyRegistration,
  REGISTRATION_CANCELLED,
  REGISTRATION_CONFIRMED,
} from '../types/course.types';

@Component({
  selector: 'app-my-registrations',
  imports: [RouterLink, ThaiDatePipe, Button, DataState],
  templateUrl: './my-registrations.html',
})
export class MyRegistrations implements OnInit {
  private readonly api = inject(CoursesApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly registrations = signal<MyRegistration[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly busyGenerationId = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.myRegistrations().subscribe({
      next: (registrations) => {
        this.registrations.set(registrations);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }

  protected isActive(registration: MyRegistration): boolean {
    return registration.status !== REGISTRATION_CANCELLED;
  }

  protected statusLabel(registration: MyRegistration): string {
    if (registration.status === REGISTRATION_CANCELLED) {
      return 'ยกเลิกแล้ว';
    }
    return registration.status === REGISTRATION_CONFIRMED ? 'ยืนยันแล้ว' : 'รอยืนยัน';
  }

  protected async cancel(registration: MyRegistration): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ยกเลิกการลงทะเบียน?',
      message: `${registration.courseTitle} ${registration.generationName ?? ''}`.trim(),
      confirmText: 'ยกเลิกการลงทะเบียน',
      cancelText: 'ไม่ยกเลิก',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.busyGenerationId.set(registration.generationId);
    this.api.cancelRegistration(registration.generationId).subscribe({
      next: () => {
        this.busyGenerationId.set(null);
        this.toast.success('ยกเลิกการลงทะเบียนแล้ว');
        this.load();
      },
      error: (error: unknown) => {
        this.busyGenerationId.set(null);
        this.toast.error(describeRegistrationError(error));
      },
    });
  }
}
