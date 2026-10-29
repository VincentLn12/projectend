import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, TemplateRef, computed, contentChildren, input, output } from '@angular/core';
import { DataState } from '../data-state/data-state';
import { CellTemplate } from './cell-template';

export interface TableColumn<T> {
  key: string;
  header: string;
  // Text for the cell. Leave it out when the column is drawn by an appCell template.
  value?: (row: T) => string | number | null | undefined;
  align?: 'left' | 'center' | 'right';
}

// A plain table with built-in loading / error / empty states. Pagination and search are separate components.
@Component({
  selector: 'app-data-table',
  imports: [NgTemplateOutlet, DataState],
  templateUrl: './data-table.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataTable<T> {
  readonly columns = input.required<TableColumn<T>[]>();
  readonly rows = input<T[]>([]);
  readonly trackBy = input<(row: T, index: number) => unknown>((_row, index) => index);
  readonly loading = input(false);
  readonly error = input<string | null>(null);
  readonly emptyMessage = input('ไม่มีข้อมูล');
  readonly retry = output<void>();

  private readonly cellTemplates = contentChildren(CellTemplate);
  private readonly templatesByKey = computed(() => {
    const map = new Map<string, TemplateRef<unknown>>();
    for (const cell of this.cellTemplates()) {
      map.set(cell.key(), cell.template);
    }
    return map;
  });

  protected templateFor(key: string): TemplateRef<unknown> | null {
    return this.templatesByKey().get(key) ?? null;
  }

  protected display(column: TableColumn<T>, row: T): string {
    const value = column.value ? column.value(row) : null;
    return value === null || value === undefined || value === '' ? '—' : String(value);
  }

  protected alignClass(column: TableColumn<T>): string {
    return column.align === 'right' ? 'text-right' : column.align === 'center' ? 'text-center' : 'text-left';
  }
}
