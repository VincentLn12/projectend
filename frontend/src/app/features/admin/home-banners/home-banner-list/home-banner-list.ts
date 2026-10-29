import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminHomeBannersApi } from '../api/home-banners.api';
import { AdminHomeBanner } from '../types/home-banner.types';

@Component({
  selector: 'app-home-banner-list',
  imports: [RouterLink, Button, DataTable, CellTemplate],
  templateUrl: './home-banner-list.html',
})
export class HomeBannerList implements OnInit {
  private readonly api = inject(AdminHomeBannersApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly rows = signal<AdminHomeBanner[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly columns: TableColumn<AdminHomeBanner>[] = [
    { key: 'title', header: 'ชื่อ' },
    { key: 'linkUrl', header: 'ลิงก์', value: (row) => row.linkUrl },
    { key: 'sortOrder', header: 'ลำดับ', value: (row) => row.sortOrder },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminHomeBanner) => row.id;

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

  protected togglePublished(row: AdminHomeBanner): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่แบนเนอร์แล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.load();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminHomeBanner): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบแบนเนอร์นี้?',
      message: `"${row.title ?? 'แบนเนอร์นี้'}" จะถูกลบออกจากหน้าแรก`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบแบนเนอร์แล้ว');
        this.load();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
