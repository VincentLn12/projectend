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
import { AdminProjectsApi } from '../api/projects.api';
import { AdminProjectListItem } from '../types/project.types';

@Component({
  selector: 'app-project-list',
  imports: [RouterLink, Button, DataTable, CellTemplate, Pagination, SearchBox],
  templateUrl: './project-list.html',
})
export class ProjectList implements OnInit {
  private readonly api = inject(AdminProjectsApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminProjectListItem>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminProjectListItem>[] = [
    { key: 'nameTH', header: 'ชื่อโครงงาน' },
    { key: 'academicYear', header: 'ปีการศึกษา', value: (row) => row.academicYear },
    { key: 'studentName', header: 'นักศึกษา', value: (row) => row.studentName },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminProjectListItem) => row.id;

  ngOnInit(): void {
    this.list.load();
  }

  protected togglePublished(row: AdminProjectListItem): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่โครงงานแล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminProjectListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบโครงงานนี้?',
      message: `"${row.nameTH}" จะถูกลบออกจากเว็บไซต์`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบโครงงานแล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
