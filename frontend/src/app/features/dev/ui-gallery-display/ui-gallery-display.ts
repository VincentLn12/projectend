import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable, delay, of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { PagedResult } from '../../../core/models/paging.models';
import { createPagedList } from '../../../shared/data/paged-list';
import {
  Button,
  CellTemplate,
  ConfirmService,
  DataState,
  DataTable,
  Pagination,
  SearchBox,
  TableColumn,
  ToastService,
} from '../../../shared/ui';

interface DemoRow {
  id: number;
  title: string;
  status: 'เผยแพร่' | 'ฉบับร่าง';
}

const ALL_ROWS: DemoRow[] = Array.from({ length: 47 }, (_unused, index) => ({
  id: index + 1,
  title: `ข่าวตัวอย่างที่ ${index + 1}`,
  status: index % 3 === 0 ? 'ฉบับร่าง' : 'เผยแพร่',
}));

// Development only (see app.routes.ts): buttons, table, pagination, search, dialogs and messages, with fake data.
@Component({
  selector: 'app-ui-gallery-display',
  imports: [RouterLink, Button, DataState, DataTable, CellTemplate, Pagination, SearchBox],
  templateUrl: './ui-gallery-display.html',
})
export class UiGalleryDisplay {
  private readonly confirm = inject(ConfirmService);
  protected readonly toast = inject(ToastService);

  protected readonly saving = signal(false);
  protected readonly stateLoading = signal(false);
  protected readonly stateError = signal<string | null>(null);
  protected readonly stateEmpty = signal(false);
  protected readonly lastAnswer = signal('ยังไม่ได้ถาม');
  protected readonly searchLog = signal<string[]>([]);
  protected readonly failNextLoad = signal(false);

  protected readonly columns: TableColumn<DemoRow>[] = [
    { key: 'id', header: '#', align: 'right', value: (row) => row.id },
    { key: 'title', header: 'หัวข้อ', value: (row) => row.title },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  // The same list state a real admin page will use, fed with fake data and a short delay.
  protected readonly list = createPagedList<DemoRow>((query) => this.fakeFetch(query.page, query.pageSize, query.search), 8);

  constructor() {
    this.list.load();
  }

  protected rowKey = (row: DemoRow): number => row.id;

  protected simulateSave(): void {
    this.saving.set(true);
    setTimeout(() => {
      this.saving.set(false);
      this.toast.success('บันทึกแล้ว');
    }, 1200);
  }

  protected async askToDelete(row: DemoRow): Promise<void> {
    const yes = await this.confirm.ask({
      title: `ลบ "${row.title}" ?`,
      message: 'ลบแล้วจะไม่แสดงบนหน้าเว็บอีก',
      confirmText: 'ลบ',
      danger: true,
    });
    this.lastAnswer.set(yes ? `ยืนยันลบ #${row.id}` : `ยกเลิกการลบ #${row.id}`);
    if (yes) {
      this.toast.success(`ลบข่าว #${row.id} แล้ว (จำลอง)`);
    }
  }

  protected onSearch(text: string): void {
    this.searchLog.update((log) => [...log, `"${text}"`].slice(-4));
    this.list.setSearch(text);
  }

  protected loadWithError(): void {
    this.failNextLoad.set(true);
    this.list.reload();
  }

  private fakeFetch(page: number, pageSize: number, search: string): Observable<PagedResult<DemoRow>> {
    if (this.failNextLoad()) {
      this.failNextLoad.set(false);
      return throwError(() => new HttpErrorResponse({ status: 500 })).pipe(delay(300));
    }

    const matches = ALL_ROWS.filter((row) => row.title.includes(search));
    const items = matches.slice((page - 1) * pageSize, page * pageSize);
    return of({ items, page, pageSize, total: matches.length }).pipe(delay(400));
  }
}
