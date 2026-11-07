import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { describeDownloadError, saveBlobAsFile } from '../../../../shared/data/download-file';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminShortCoursesApi } from '../api/short-courses.api';
import { REGISTRATION_CANCELLED, REGISTRATION_LABELS, ShortCourseRegistration } from '../types/short-course.types';

@Component({
  selector: 'app-short-course-registrations',
  imports: [RouterLink, ThaiDatePipe, Button, DataTable, CellTemplate],
  templateUrl: './short-course-registrations.html',
})
export class ShortCourseRegistrations implements OnInit {
  private readonly api = inject(AdminShortCoursesApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);

  protected readonly courseId = this.route.snapshot.paramMap.get('id')!;

  protected readonly courseTitle = signal('');
  protected readonly rows = signal<ShortCourseRegistration[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly downloading = signal<string | null>(null);

  protected readonly columns: TableColumn<ShortCourseRegistration>[] = [
    { key: 'index', header: 'ที่', align: 'center' },
    { key: 'code', header: 'รหัสการสมัคร', value: (row) => row.code },
    { key: 'name', header: 'ชื่อ-นามสกุล' },
    { key: 'phone', header: 'เบอร์โทร', value: (row) => row.phone },
    { key: 'status', header: 'สถานะ', value: (row) => REGISTRATION_LABELS[row.status] },
    { key: 'registeredAt', header: 'วันที่สมัคร' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: ShortCourseRegistration) => row.id;

  ngOnInit(): void {
    this.api.get(this.courseId).subscribe({ next: (course) => this.courseTitle.set(course.title) });
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.registrations(this.courseId).subscribe({
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

  protected fullName(row: ShortCourseRegistration): string {
    return `${row.prefix ?? ''}${row.firstName} ${row.lastName}`;
  }

  protected rowNumber(row: ShortCourseRegistration): number {
    return this.rows().indexOf(row) + 1;
  }

  protected isActive(row: ShortCourseRegistration): boolean {
    return row.status !== REGISTRATION_CANCELLED;
  }

  protected downloadSignInSheet(): void {
    this.download('sign-in', this.api.signInSheet(this.courseId), 'ใบลงชื่อ.pdf');
  }

  protected downloadAllCertificates(): void {
    this.download('certificates', this.api.certificates(this.courseId), 'เกียรติบัตรทั้งหมด.pdf');
  }

  protected downloadCertificate(row: ShortCourseRegistration): void {
    this.download(row.id, this.api.certificate(this.courseId, row.id), `เกียรติบัตร-${row.firstName}.pdf`);
  }

  protected async cancel(row: ShortCourseRegistration): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ยกเลิกการสมัครของผู้นี้?',
      message: `${this.fullName(row)} (${row.code})`,
      confirmText: 'ยกเลิกการสมัคร',
      cancelText: 'ไม่ยกเลิก',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.cancelRegistration(this.courseId, row.id).subscribe({
      next: () => {
        this.toast.success('ยกเลิกการสมัครแล้ว');
        this.load();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  private download(key: string, request: ReturnType<AdminShortCoursesApi['signInSheet']>, fileName: string): void {
    if (this.downloading()) {
      return;
    }
    this.downloading.set(key);
    request.subscribe({
      next: (blob) => {
        saveBlobAsFile(blob, fileName);
        this.downloading.set(null);
      },
      error: async (error: unknown) => {
        this.downloading.set(null);
        this.toast.error(await describeDownloadError(error));
      },
    });
  }
}
