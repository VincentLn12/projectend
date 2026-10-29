import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { createPagedList } from '../../../../shared/data/paged-list';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminLecturersApi } from '../api/lecturers.api';
import { AdminLecturer } from '../types/lecturer.types';

@Component({
  selector: 'app-lecturer-list',
  imports: [RouterLink, Button, DataTable, CellTemplate, Pagination, SearchBox],
  templateUrl: './lecturer-list.html',
})
export class LecturerList implements OnInit {
  private readonly api = inject(AdminLecturersApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminLecturer>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminLecturer>[] = [
    { key: 'fullName', header: 'ชื่อ-สกุล' },
    { key: 'position', header: 'ตำแหน่ง', value: (row) => row.position },
    { key: 'sortOrder', header: 'ลำดับ', value: (row) => row.sortOrder },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminLecturer) => row.id;

  ngOnInit(): void {
    this.list.load();
  }

  protected togglePublished(row: AdminLecturer): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่อาจารย์แล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminLecturer): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบอาจารย์นี้?',
      message: `"${row.prefix ?? ''}${row.fullName}" จะถูกลบออกจากหน้าอาจารย์ของเว็บไซต์`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบอาจารย์แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
