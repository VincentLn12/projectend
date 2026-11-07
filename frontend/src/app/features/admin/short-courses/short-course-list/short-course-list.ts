import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { createPagedList } from '../../../../shared/data/paged-list';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminShortCoursesApi } from '../api/short-courses.api';
import { AdminShortCourseListItem, AVAILABILITY_LABELS } from '../types/short-course.types';

@Component({
  selector: 'app-short-course-list',
  imports: [RouterLink, ThaiDatePipe, Button, DataTable, CellTemplate, Pagination, SearchBox],
  templateUrl: './short-course-list.html',
})
export class ShortCourseList implements OnInit {
  private readonly api = inject(AdminShortCoursesApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminShortCourseListItem>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminShortCourseListItem>[] = [
    { key: 'title', header: 'ชื่อหลักสูตร' },
    { key: 'startDate', header: 'วันที่อบรม' },
    { key: 'seats', header: 'ผู้สมัคร', align: 'center' },
    { key: 'availability', header: 'การรับสมัคร', value: (row) => AVAILABILITY_LABELS[row.availability] },
    { key: 'status', header: 'การเผยแพร่' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminShortCourseListItem) => row.id;

  ngOnInit(): void {
    this.list.load();
  }

  protected seatsText(row: AdminShortCourseListItem): string {
    return row.maxParticipants > 0 ? `${row.registeredCount} / ${row.maxParticipants}` : `${row.registeredCount} / ไม่จำกัด`;
  }

  protected togglePublished(row: AdminShortCourseListItem): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่หลักสูตรแล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminShortCourseListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบหลักสูตรนี้?',
      message: `"${row.title}" จะถูกลบออกจากเว็บไซต์ (ลบไม่ได้ถ้ายังมีผู้สมัครอยู่)`,
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
