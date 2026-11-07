import { Component, inject, OnInit, signal } from '@angular/core';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminStudentApprovalsApi } from '../api/student-approvals.api';
import { PendingStudent } from '../types/student-approval.types';

@Component({
  selector: 'app-student-approval-list',
  imports: [ThaiDatePipe, Button, DataTable, CellTemplate],
  templateUrl: './student-approval-list.html',
})
export class StudentApprovalList implements OnInit {
  private readonly api = inject(AdminStudentApprovalsApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly rows = signal<PendingStudent[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly columns: TableColumn<PendingStudent>[] = [
    { key: 'studentCode', header: 'รหัสนักศึกษา', value: (row) => row.studentCode },
    { key: 'fullName', header: 'ชื่อ-นามสกุล', value: (row) => row.fullName },
    { key: 'email', header: 'อีเมล', value: (row) => row.email },
    { key: 'entryYear', header: 'ปีที่เข้าเรียน', value: (row) => row.entryYear },
    { key: 'registeredAt', header: 'สมัครเมื่อ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: PendingStudent) => row.userId;

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.list().subscribe({
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

  protected async approve(row: PendingStudent): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'อนุมัตินักศึกษาคนนี้?',
      message: `${row.fullName} รหัส ${row.studentCode} จะใช้ระบบโครงงานนักศึกษาได้ ตรวจให้แน่ใจว่าเป็นเจ้าของรหัสตัวจริง`,
      confirmText: 'อนุมัติ',
    });
    if (!confirmed) {
      return;
    }

    this.api.approve(row.userId).subscribe({
      next: () => {
        this.toast.success('อนุมัติแล้ว');
        this.load();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async reject(row: PendingStudent): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ปฏิเสธและลบคำขอนี้?',
      message: `${row.fullName} รหัส ${row.studentCode} จะถูกลบ และเจ้าของรหัสตัวจริงสมัครใหม่ได้`,
      confirmText: 'ปฏิเสธและลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.reject(row.userId).subscribe({
      next: () => {
        this.toast.success('ลบคำขอแล้ว');
        this.load();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
