import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { AdminMenuApi, describeMenuError } from '../api/menu.api';
import { AdminMenuItem, KIND_GROUP, KIND_LABELS, MenuRow } from '../types/menu.types';

@Component({
  selector: 'app-menu-list',
  imports: [RouterLink, Button, DataTable, CellTemplate],
  templateUrl: './menu-list.html',
})
export class MenuList implements OnInit {
  private readonly api = inject(AdminMenuApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly items = signal<AdminMenuItem[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly busy = signal(false);

  // Main items in order, each followed by its sub-menu items.
  protected readonly rows = computed<MenuRow[]>(() => {
    const all = this.items();
    const bySort = (a: AdminMenuItem, b: AdminMenuItem) => a.sortOrder - b.sortOrder;
    const topLevel = all.filter((item) => item.parentId === null).sort(bySort);
    const rows: MenuRow[] = [];

    topLevel.forEach((item, index) => {
      rows.push({ item, depth: 0, canMoveUp: index > 0, canMoveDown: index < topLevel.length - 1 });

      const children = all.filter((child) => child.parentId === item.id).sort(bySort);
      children.forEach((child, childIndex) => {
        rows.push({ item: child, depth: 1, canMoveUp: childIndex > 0, canMoveDown: childIndex < children.length - 1 });
      });
    });

    return rows;
  });

  protected readonly columns: TableColumn<MenuRow>[] = [
    { key: 'label', header: 'ชื่อเมนู' },
    { key: 'kind', header: 'ประเภท', value: (row) => KIND_LABELS[row.item.kind] },
    { key: 'target', header: 'ปลายทาง' },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: MenuRow) => row.item.id;
  protected readonly kindGroup = KIND_GROUP;

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.list().subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }

  protected move(row: MenuRow, direction: -1 | 1): void {
    const siblings = this.rows()
      .filter((other) => other.item.parentId === row.item.parentId)
      .map((other) => other.item.id);
    const position = siblings.indexOf(row.item.id);
    const target = position + direction;
    if (target < 0 || target >= siblings.length) {
      return;
    }

    [siblings[position], siblings[target]] = [siblings[target], siblings[position]];
    this.run(this.api.reorder(row.item.parentId, siblings), null);
  }

  protected toggleVisible(row: MenuRow): void {
    const show = !row.item.isVisible;
    this.run(this.api.setVisible(row.item.id, show), show ? 'แสดงเมนูแล้ว' : 'ซ่อนเมนูแล้ว');
  }

  protected async askToDelete(row: MenuRow): Promise<void> {
    const isGroup = row.item.kind === KIND_GROUP;
    const confirmed = await this.confirm.ask({
      title: 'ลบเมนูนี้?',
      message: isGroup
        ? `"${row.item.label}" และเมนูย่อยทั้งหมดในกลุ่มจะถูกลบออกจากเว็บไซต์`
        : `"${row.item.label}" จะถูกลบออกจากเมนูของเว็บไซต์`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (confirmed) {
      this.run(this.api.delete(row.item.id), 'ลบเมนูแล้ว');
    }
  }

  protected async askToReset(): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'คืนค่าเมนูเริ่มต้น?',
      message: 'เมนูที่แก้ไขและเพิ่มไว้ทั้งหมดจะถูกลบ แล้วใช้เมนูเริ่มต้นของระบบแทน',
      confirmText: 'คืนค่าเริ่มต้น',
      danger: true,
    });
    if (confirmed) {
      this.run(this.api.reset(), 'คืนค่าเมนูเริ่มต้นแล้ว');
    }
  }

  protected targetText(row: MenuRow): string {
    return row.item.kind === KIND_GROUP ? '—' : (row.item.target ?? '—');
  }

  private run(call: Observable<unknown>, successMessage: string | null): void {
    this.busy.set(true);
    call.subscribe({
      next: () => {
        this.busy.set(false);
        if (successMessage) {
          this.toast.success(successMessage);
        }
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.toast.error(describeMenuError(error));
      },
    });
  }
}
