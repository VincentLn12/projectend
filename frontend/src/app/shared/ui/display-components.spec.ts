import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { Button } from './button/button';
import { CellTemplate } from './data-table/cell-template';
import { DataState } from './data-state/data-state';
import { DataTable, TableColumn } from './data-table/data-table';
import { Pagination, buildPageList } from './pagination/pagination';
import { SearchBox } from './search-box/search-box';

function query<E extends Element>(fixture: ComponentFixture<unknown>, selector: string): E {
  return fixture.nativeElement.querySelector(selector) as E;
}

function queryAll<E extends Element>(fixture: ComponentFixture<unknown>, selector: string): E[] {
  return Array.from(fixture.nativeElement.querySelectorAll(selector)) as E[];
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

// ---------------------------------------------------------------------------------------------------------------------

@Component({
  imports: [Button],
  template: `
    <button app-button variant="danger" size="sm" [loading]="loading()">ลบ</button>
    <a app-button variant="secondary" href="/somewhere">ไปต่อ</a>
  `,
})
class ButtonHost {
  loading = signal(false);
}

describe('Button', () => {
  it('looks like its variant and size, on a button and on a link', async () => {
    const fixture = TestBed.createComponent(ButtonHost);
    await settle(fixture);

    expect(query(fixture, 'button').className).toContain('bg-red-600');
    expect(query(fixture, 'button').className).toContain('text-sm');
    expect(query(fixture, 'a').className).toContain('border-slate-400');
  });

  it('while loading: shows a spinner, says it is busy and cannot be clicked', async () => {
    const fixture = TestBed.createComponent(ButtonHost);
    await settle(fixture);
    expect(query(fixture, 'button svg')).toBeNull();
    expect(query<HTMLButtonElement>(fixture, 'button').disabled).toBe(false);

    fixture.componentInstance.loading.set(true);
    await settle(fixture);

    expect(query(fixture, 'button svg')).not.toBeNull();
    expect(query<HTMLButtonElement>(fixture, 'button').disabled).toBe(true);
    expect(query(fixture, 'button').getAttribute('aria-busy')).toBe('true');
  });

  it('can be disabled with [disabled] or a plain disabled attribute, and loading does not undo that', async () => {
    @Component({
      imports: [Button],
      template: `
        <button id="bound" app-button [disabled]="off()">ก</button>
        <button id="plain" app-button disabled>ข</button>
        <button id="free" app-button>ค</button>
        <button id="both" app-button [disabled]="true" [loading]="false">ง</button>
      `,
    })
    class DisabledHost {
      off = signal(true);
    }
    const fixture = TestBed.createComponent(DisabledHost);
    await settle(fixture);
    const isDisabled = (id: string) => query<HTMLButtonElement>(fixture, `#${id}`).disabled;

    expect([isDisabled('bound'), isDisabled('plain'), isDisabled('free'), isDisabled('both')]).toEqual([true, true, false, true]);

    fixture.componentInstance.off.set(false);
    await settle(fixture);
    expect(isDisabled('bound')).toBe(false);
  });

  it('is enabled again when loading ends', async () => {
    const fixture = TestBed.createComponent(ButtonHost);
    fixture.componentInstance.loading.set(true);
    await settle(fixture);

    fixture.componentInstance.loading.set(false);
    await settle(fixture);

    expect(query<HTMLButtonElement>(fixture, 'button').disabled).toBe(false);
    expect(query(fixture, 'button').getAttribute('aria-busy')).toBeNull();
  });
});

// ---------------------------------------------------------------------------------------------------------------------

@Component({
  imports: [DataState],
  template: `
    <app-data-state
      [loading]="loading()"
      [error]="error()"
      [empty]="empty()"
      emptyMessage="ยังไม่มีข่าว"
      (retry)="retries = retries + 1"
    >
      <p id="content">เนื้อหา</p>
    </app-data-state>
  `,
})
class StateHost {
  loading = signal(false);
  error = signal<string | null>(null);
  empty = signal(false);
  retries = 0;
}

describe('DataState', () => {
  it('shows the content when nothing is wrong', async () => {
    const fixture = TestBed.createComponent(StateHost);
    await settle(fixture);

    expect(query(fixture, '#content')).not.toBeNull();
    expect(query(fixture, '[role="status"]')).toBeNull();
    expect(query(fixture, '[role="alert"]')).toBeNull();
  });

  it('shows a spinner instead of the content while loading', async () => {
    const fixture = TestBed.createComponent(StateHost);
    fixture.componentInstance.loading.set(true);
    await settle(fixture);

    expect(query(fixture, '[role="status"]').textContent).toContain('กำลังโหลด');
    expect(query(fixture, '#content')).toBeNull();
  });

  it('shows the error with a retry button that tells the page', async () => {
    const fixture = TestBed.createComponent(StateHost);
    fixture.componentInstance.error.set('โหลดข้อมูลไม่สำเร็จ');
    await settle(fixture);

    expect(query(fixture, '[role="alert"]').textContent).toContain('โหลดข้อมูลไม่สำเร็จ');
    query<HTMLButtonElement>(fixture, '[role="alert"] button').click();
    expect(fixture.componentInstance.retries).toBe(1);
  });

  it('shows the empty message', async () => {
    const fixture = TestBed.createComponent(StateHost);
    fixture.componentInstance.empty.set(true);
    await settle(fixture);

    expect(fixture.nativeElement.textContent).toContain('ยังไม่มีข่าว');
    expect(query(fixture, '#content')).toBeNull();
  });

  it('loading wins over an error, and an error wins over empty', async () => {
    const fixture = TestBed.createComponent(StateHost);
    fixture.componentInstance.loading.set(true);
    fixture.componentInstance.error.set('x');
    fixture.componentInstance.empty.set(true);
    await settle(fixture);
    expect(query(fixture, '[role="status"]')).not.toBeNull();
    expect(query(fixture, '[role="alert"]')).toBeNull();

    fixture.componentInstance.loading.set(false);
    await settle(fixture);
    expect(query(fixture, '[role="alert"]')).not.toBeNull();
  });
});

// ---------------------------------------------------------------------------------------------------------------------

interface Row {
  id: number;
  title: string;
  note: string | null;
}

@Component({
  imports: [DataTable, CellTemplate],
  template: `
    <app-data-table
      [columns]="columns"
      [rows]="rows()"
      [trackBy]="rowKey"
      [loading]="loading()"
      [error]="error()"
      emptyMessage="ไม่พบข่าว"
      (retry)="retries = retries + 1"
    >
      <ng-template appCell="actions" let-row let-index="index">
        <button type="button" class="delete" (click)="deleted.push(row.id)">ลบ {{ row.title }} ({{ index }})</button>
      </ng-template>
    </app-data-table>
  `,
})
class TableHost {
  columns: TableColumn<Row>[] = [
    { key: 'id', header: '#', align: 'right', value: (row) => row.id },
    { key: 'title', header: 'หัวข้อ', value: (row) => row.title },
    { key: 'note', header: 'หมายเหตุ', value: (row) => row.note },
    { key: 'actions', header: '' },
  ];
  rows = signal<Row[]>([
    { id: 1, title: 'ข่าวแรก', note: null },
    { id: 2, title: 'ข่าวสอง', note: 'สำคัญ' },
  ]);
  rowKey = (row: Row) => row.id;
  loading = signal(false);
  error = signal<string | null>(null);
  retries = 0;
  deleted: number[] = [];
}

describe('DataTable', () => {
  it('draws the headers and one row per item', async () => {
    const fixture = TestBed.createComponent(TableHost);
    await settle(fixture);

    expect(queryAll(fixture, 'th').map((th) => th.textContent?.trim())).toEqual(['#', 'หัวข้อ', 'หมายเหตุ', '']);
    expect(queryAll(fixture, 'tbody tr').length).toBe(2);
    expect(queryAll(fixture, 'tbody tr')[0].textContent).toContain('ข่าวแรก');
  });

  it('shows a dash for an empty value and aligns a column to the right', async () => {
    const fixture = TestBed.createComponent(TableHost);
    await settle(fixture);

    const firstRowCells = queryAll(fixture, 'tbody tr')[0].querySelectorAll('td');
    expect(firstRowCells[2].textContent?.trim()).toBe('—');
    expect(firstRowCells[0].className).toContain('text-right');
  });

  it('draws a column with the page\'s own template, giving it the row and its position', async () => {
    const fixture = TestBed.createComponent(TableHost);
    await settle(fixture);

    const buttons = queryAll<HTMLButtonElement>(fixture, 'button.delete');
    expect(buttons.map((b) => b.textContent?.trim())).toEqual(['ลบ ข่าวแรก (0)', 'ลบ ข่าวสอง (1)']);

    buttons[1].click();
    expect(fixture.componentInstance.deleted).toEqual([2]);
  });

  it('shows the empty message instead of an empty table', async () => {
    const fixture = TestBed.createComponent(TableHost);
    fixture.componentInstance.rows.set([]);
    await settle(fixture);

    expect(query(fixture, 'table')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('ไม่พบข่าว');
  });

  it('shows loading and error states, and passes on the retry', async () => {
    const fixture = TestBed.createComponent(TableHost);
    fixture.componentInstance.loading.set(true);
    await settle(fixture);
    expect(query(fixture, '[role="status"]')).not.toBeNull();
    expect(query(fixture, 'table')).toBeNull();

    fixture.componentInstance.loading.set(false);
    fixture.componentInstance.error.set('โหลดไม่สำเร็จ');
    await settle(fixture);
    query<HTMLButtonElement>(fixture, '[role="alert"] button').click();

    expect(fixture.componentInstance.retries).toBe(1);
  });
});

// ---------------------------------------------------------------------------------------------------------------------

describe('buildPageList', () => {
  it('lists every page when there are few', () => {
    expect(buildPageList(1, 1)).toEqual([1]);
    expect(buildPageList(3, 5)).toEqual([1, 2, 3, 4, 5]);
    expect(buildPageList(4, 7)).toEqual([1, 2, 3, 4, 5, 6, 7]);
  });

  it('keeps the first, last and the neighbours of the current page, with gaps marked null', () => {
    expect(buildPageList(1, 20)).toEqual([1, 2, null, 20]);
    expect(buildPageList(10, 20)).toEqual([1, null, 9, 10, 11, null, 20]);
    expect(buildPageList(20, 20)).toEqual([1, null, 19, 20]);
    expect(buildPageList(3, 20)).toEqual([1, 2, 3, 4, null, 20]);
  });
});

@Component({
  imports: [Pagination],
  template: `<app-pagination [page]="page()" [pageSize]="pageSize()" [total]="total()" (pageChange)="requested.push($event)" />`,
})
class PaginationHost {
  page = signal(1);
  pageSize = signal(10);
  total = signal(45);
  requested: number[] = [];
}

describe('Pagination', () => {
  const buttonWithText = (fixture: ComponentFixture<unknown>, text: string) =>
    queryAll<HTMLButtonElement>(fixture, 'button').find((b) => b.textContent?.trim() === text)!;

  it('says which rows are shown', async () => {
    const fixture = TestBed.createComponent(PaginationHost);
    await settle(fixture);
    expect(fixture.nativeElement.textContent).toContain('แสดง 1–10 จาก 45 รายการ');

    fixture.componentInstance.page.set(5);
    await settle(fixture);
    expect(fixture.nativeElement.textContent).toContain('แสดง 41–45 จาก 45 รายการ');
  });

  it('asks for the page that was clicked, and marks the current one', async () => {
    const fixture = TestBed.createComponent(PaginationHost);
    await settle(fixture);

    buttonWithText(fixture, '3').click();

    expect(fixture.componentInstance.requested).toEqual([3]);
    expect(buttonWithText(fixture, '1').getAttribute('aria-current')).toBe('page');
    expect(buttonWithText(fixture, '3').getAttribute('aria-current')).toBeNull();
  });

  it('previous is disabled on the first page and next on the last', async () => {
    const fixture = TestBed.createComponent(PaginationHost);
    await settle(fixture);
    expect(buttonWithText(fixture, 'ก่อนหน้า').disabled).toBe(true);
    expect(buttonWithText(fixture, 'ถัดไป').disabled).toBe(false);

    buttonWithText(fixture, 'ถัดไป').click();
    expect(fixture.componentInstance.requested).toEqual([2]);

    fixture.componentInstance.page.set(5);
    await settle(fixture);
    expect(buttonWithText(fixture, 'ถัดไป').disabled).toBe(true);
  });

  it('does not ask for the page it is already on', async () => {
    const fixture = TestBed.createComponent(PaginationHost);
    await settle(fixture);

    buttonWithText(fixture, '1').click();

    expect(fixture.componentInstance.requested).toEqual([]);
  });

  it('shows no buttons for one page, and a message for no rows', async () => {
    const fixture = TestBed.createComponent(PaginationHost);
    fixture.componentInstance.total.set(7);
    await settle(fixture);
    expect(query(fixture, 'nav')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('แสดง 1–7 จาก 7 รายการ');

    fixture.componentInstance.total.set(0);
    await settle(fixture);
    expect(fixture.nativeElement.textContent).toContain('ไม่มีรายการ');
  });
});

// ---------------------------------------------------------------------------------------------------------------------

@Component({
  imports: [SearchBox],
  template: `<app-search-box [value]="initial" (searchChange)="heard.push($event)" />`,
})
class SearchHost {
  initial = '';
  heard: string[] = [];
}

describe('SearchBox', () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  const type = (fixture: ComponentFixture<unknown>, text: string) => {
    const input = query<HTMLInputElement>(fixture, 'input');
    input.value = text;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  };

  it('waits until the person stops typing, and announces only the last text', () => {
    const fixture = TestBed.createComponent(SearchHost);
    fixture.detectChanges();

    type(fixture, 'a');
    vi.advanceTimersByTime(200);
    type(fixture, 'ab');
    vi.advanceTimersByTime(200);
    expect(fixture.componentInstance.heard).toEqual([]);

    vi.advanceTimersByTime(150);
    expect(fixture.componentInstance.heard).toEqual(['ab']);
  });

  it('Enter announces at once, and the same text is not announced twice', () => {
    const fixture = TestBed.createComponent(SearchHost);
    fixture.detectChanges();

    type(fixture, 'ข่าว');
    query<HTMLInputElement>(fixture, 'input').dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));
    expect(fixture.componentInstance.heard).toEqual(['ข่าว']);

    vi.advanceTimersByTime(1000);
    expect(fixture.componentInstance.heard).toEqual(['ข่าว']);
  });

  it('trims the text', () => {
    const fixture = TestBed.createComponent(SearchHost);
    fixture.detectChanges();

    type(fixture, '  abc  ');
    vi.advanceTimersByTime(400);

    expect(fixture.componentInstance.heard).toEqual(['abc']);
  });

  it('the clear button empties the box and announces the empty search', () => {
    const fixture = TestBed.createComponent(SearchHost);
    fixture.detectChanges();
    type(fixture, 'abc');
    vi.advanceTimersByTime(400);

    query<HTMLButtonElement>(fixture, 'button').click();
    fixture.detectChanges();

    expect(query<HTMLInputElement>(fixture, 'input').value).toBe('');
    expect(fixture.componentInstance.heard).toEqual(['abc', '']);
    expect(query(fixture, 'button')).toBeNull();
  });

  it('starts from the value it is given, and does not announce it again', () => {
    const fixture = TestBed.createComponent(SearchHost);
    fixture.componentInstance.initial = 'เดิม';
    fixture.detectChanges();
    expect(query<HTMLInputElement>(fixture, 'input').value).toBe('เดิม');

    type(fixture, 'เดิม');
    vi.advanceTimersByTime(400);

    expect(fixture.componentInstance.heard).toEqual([]);
  });
});
