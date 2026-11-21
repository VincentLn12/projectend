import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminCertificateTemplatesApi } from '../api/certificate-templates.api';
import { CertificateTemplate } from '../types/certificate-template.types';

@Component({
  selector: 'app-certificate-template-list',
  imports: [RouterLink, Button, DataTable, CellTemplate],
  templateUrl: './certificate-template-list.html',
})
export class CertificateTemplateList implements OnInit {
  private readonly api = inject(AdminCertificateTemplatesApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly rows = signal<CertificateTemplate[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly columns: TableColumn<CertificateTemplate>[] = [
    { key: 'name', header: 'ชื่อแม่แบบ' },
    { key: 'paper', header: 'กระดาษ' },
    { key: 'elementCount', header: 'องค์ประกอบ', value: (row) => row.elements.length },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: CertificateTemplate) => row.id;

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

  protected paperText(row: CertificateTemplate): string {
    return `${row.pageSize} ${row.landscape ? 'แนวนอน' : 'แนวตั้ง'}`;
  }

  protected async askToDelete(row: CertificateTemplate): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบแม่แบบนี้?',
      message: `"${row.name}" (ลบไม่ได้ถ้ายังมีหลักสูตรใช้อยู่)`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบแม่แบบแล้ว');
        this.load();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
