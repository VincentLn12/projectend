import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { createPagedList } from '../../../../shared/data/paged-list';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminSyllabusesApi } from '../api/syllabuses.api';
import { AdminSyllabusListItem } from '../types/syllabus.types';

@Component({
  selector: 'app-syllabus-list',
  imports: [RouterLink, Button, DataTable, CellTemplate, Pagination],
  templateUrl: './syllabus-list.html',
})
export class SyllabusList implements OnInit {
  private readonly api = inject(AdminSyllabusesApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminSyllabusListItem>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminSyllabusListItem>[] = [
    { key: 'nameTH', header: 'ชื่อหลักสูตร' },
    { key: 'year', header: 'ฉบับปี พ.ศ.', value: (row) => row.year },
    { key: 'current', header: 'ฉบับปัจจุบัน' },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminSyllabusListItem) => row.id;

  ngOnInit(): void {
    this.list.load();
  }

  protected togglePublished(row: AdminSyllabusListItem): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่หลักสูตรแล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminSyllabusListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบหลักสูตรนี้?',
      message: `"${row.nameTH}" ฉบับ พ.ศ. ${row.year} พร้อมรายวิชาทั้งหมดจะถูกลบ`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบหลักสูตรแล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
