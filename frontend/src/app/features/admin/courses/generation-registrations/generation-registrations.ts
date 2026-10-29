import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminCoursesApi } from '../api/courses.api';
import { REGISTRATION_CANCELLED, REGISTRATION_LABELS, RegistrationItem } from '../types/course.types';

@Component({
  selector: 'app-generation-registrations',
  imports: [RouterLink, ThaiDatePipe, Button, DataTable, CellTemplate],
  templateUrl: './generation-registrations.html',
})
export class GenerationRegistrations implements OnInit {
  private readonly api = inject(AdminCoursesApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);

  protected readonly courseId = this.route.snapshot.paramMap.get('courseId')!;
  private readonly generationId = this.route.snapshot.paramMap.get('id')!;

  protected readonly rows = signal<RegistrationItem[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly columns: TableColumn<RegistrationItem>[] = [
    { key: 'fullName', header: 'ชื่อ-สกุล', value: (row) => row.fullName },
    { key: 'email', header: 'อีเมล', value: (row) => row.email },
    { key: 'status', header: 'สถานะ', value: (row) => REGISTRATION_LABELS[row.status] },
    { key: 'registeredAt', header: 'วันที่ลงทะเบียน' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: RegistrationItem) => row.userId;

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.registrations(this.generationId).subscribe({
      next: (rows) => {
        this.rows.set(rows);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }

  protected isActive(row: RegistrationItem): boolean {
    return row.status !== REGISTRATION_CANCELLED;
  }

  protected async cancel(row: RegistrationItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ยกเลิกการลงทะเบียนของผู้ใช้นี้?',
      message: `${row.fullName} (${row.email})`,
      confirmText: 'ยกเลิกการลงทะเบียน',
      cancelText: 'ไม่ยกเลิก',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.cancelRegistration(this.generationId, row.userId).subscribe({
      next: () => {
        this.toast.success('ยกเลิกการลงทะเบียนแล้ว');
        this.load();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
