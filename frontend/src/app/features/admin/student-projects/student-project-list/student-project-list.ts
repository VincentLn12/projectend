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
import { AdminStudentProjectsApi } from '../api/student-projects.api';
import { AdminStudentProjectListItem, SEMESTER_LABELS } from '../types/student-project.types';

@Component({
  selector: 'app-admin-student-project-list',
  imports: [RouterLink, Button, DataTable, CellTemplate, Pagination, SearchBox],
  templateUrl: './student-project-list.html',
})
export class AdminStudentProjectList implements OnInit {
  private readonly api = inject(AdminStudentProjectsApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminStudentProjectListItem>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminStudentProjectListItem>[] = [
    { key: 'title', header: 'ชื่อโปรเจค', value: (row) => row.title },
    { key: 'studentName', header: 'นักศึกษา', value: (row) => row.studentName },
    { key: 'term', header: 'ปีการศึกษา / ภาคเรียน', value: (row) => `${row.academicYear} · ${SEMESTER_LABELS[row.semester]}` },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminStudentProjectListItem) => row.id;

  ngOnInit(): void {
    this.list.load();
  }

  protected togglePublished(row: AdminStudentProjectListItem): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่โปรเจคแล้ว' : 'ซ่อนโปรเจคแล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminStudentProjectListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบโปรเจคนี้?',
      message: `"${row.title}" ของ ${row.studentName} จะถูกลบ`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบโปรเจคแล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
