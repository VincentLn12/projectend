import { DatePipe } from '@angular/common';
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
import { AdminNewsApi } from '../api/news.api';
import { AdminNewsListItem } from '../types/news.types';

@Component({
  selector: 'app-news-list',
  imports: [RouterLink, DatePipe, Button, DataTable, CellTemplate, Pagination, SearchBox],
  templateUrl: './news-list.html',
})
export class NewsList implements OnInit {
  private readonly api = inject(AdminNewsApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminNewsListItem>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminNewsListItem>[] = [
    { key: 'title', header: 'หัวข้อ' },
    { key: 'status', header: 'สถานะ' },
    { key: 'publishedAt', header: 'วันที่เผยแพร่' },
    { key: 'authorName', header: 'ผู้เขียน', value: (row) => row.authorName },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminNewsListItem) => row.id;

  ngOnInit(): void {
    this.list.load();
  }

  protected togglePublished(row: AdminNewsListItem): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่ข่าวแล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminNewsListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบข่าวนี้?',
      message: `"${row.title}" จะถูกลบออกจากเว็บไซต์`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบข่าวแล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
