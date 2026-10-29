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
import { AdminCoursesApi } from '../api/courses.api';
import { AdminCourseListItem } from '../types/course.types';

@Component({
  selector: 'app-course-list',
  imports: [RouterLink, Button, DataTable, CellTemplate, Pagination, SearchBox],
  templateUrl: './course-list.html',
})
export class CourseList implements OnInit {
  private readonly api = inject(AdminCoursesApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminCourseListItem>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminCourseListItem>[] = [
    { key: 'title', header: 'ชื่อหลักสูตรระยะสั้น' },
    { key: 'lecturerName', header: 'วิทยากร', value: (row) => row.lecturerName },
    { key: 'generationCount', header: 'จำนวนรุ่น', value: (row) => row.generationCount, align: 'center' },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminCourseListItem) => row.id;

  ngOnInit(): void {
    this.list.load();
  }

  protected togglePublished(row: AdminCourseListItem): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่หลักสูตรแล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminCourseListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบหลักสูตรนี้?',
      message: `"${row.title}" และรุ่นอบรมทั้งหมดจะถูกลบออกจากเว็บไซต์`,
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
