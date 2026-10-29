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
import { AdminJobHistoriesApi } from '../api/job-histories.api';
import { AdminJobHistory } from '../types/job-history.types';

@Component({
  selector: 'app-job-history-list',
  imports: [RouterLink, Button, DataTable, CellTemplate, Pagination, SearchBox],
  templateUrl: './job-history-list.html',
})
export class JobHistoryList implements OnInit {
  private readonly api = inject(AdminJobHistoriesApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly list = createPagedList<AdminJobHistory>((query) => this.api.list(query));

  protected readonly columns: TableColumn<AdminJobHistory>[] = [
    { key: 'jobName', header: 'อาชีพ/งาน' },
    { key: 'personName', header: 'บัณฑิต', value: (row) => row.personName },
    { key: 'company', header: 'สถานที่ทำงาน', value: (row) => row.company },
    { key: 'startDate', header: 'เริ่มงาน', value: (row) => row.startDate },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminJobHistory) => row.id;

  ngOnInit(): void {
    this.list.load();
  }

  protected togglePublished(row: AdminJobHistory): void {
    const publish = !row.isPublished;
    this.api.setPublished(row.id, publish).subscribe({
      next: () => {
        this.toast.success(publish ? 'เผยแพร่แล้ว' : 'ยกเลิกการเผยแพร่แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(row: AdminJobHistory): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบประวัติการทำงานนี้?',
      message: `"${row.jobName}" จะถูกลบออกจากเว็บไซต์`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบประวัติการทำงานแล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
