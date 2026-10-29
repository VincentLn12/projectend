import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminTechSubjectsApi } from '../api/tech-subjects.api';
import { AdminTechSubject } from '../types/tech-subject.types';

@Component({
  selector: 'app-tech-subject-list',
  imports: [RouterLink, Button, DataTable, CellTemplate],
  templateUrl: './tech-subject-list.html',
})
export class TechSubjectList implements OnInit {
  private readonly api = inject(AdminTechSubjectsApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly rows = signal<AdminTechSubject[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly columns: TableColumn<AdminTechSubject>[] = [
    { key: 'subjectName', header: 'ชื่อ' },
    { key: 'subTitle', header: 'คำอธิบายสั้น', value: (row) => row.subTitle },
    { key: 'sortOrder', header: 'ลำดับ', value: (row) => row.sortOrder },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminTechSubject) => row.id;

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

  protected togglePublished(row: AdminTechSubject): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่เทคโนโลยีแล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.load();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminTechSubject): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบเทคโนโลยีนี้?',
      message: `"${row.subjectName}" จะถูกลบออกจากเว็บไซต์`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบเทคโนโลยีแล้ว');
        this.load();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
