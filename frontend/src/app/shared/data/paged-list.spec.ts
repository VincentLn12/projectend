import { HttpErrorResponse } from '@angular/common/http';
import { Subject } from 'rxjs';
import { PagedResult } from '../../core/models/paging.models';
import { describeHttpError } from '../forms/server-errors';
import { PagedList, PagedQuery } from './paged-list';

const result = (items: string[], page: number, total: number): PagedResult<string> => ({ items, page, pageSize: 10, total });

describe('PagedList', () => {
  let requests: { query: PagedQuery; response: Subject<PagedResult<string>> }[];
  let list: PagedList<string>;

  beforeEach(() => {
    requests = [];
    list = new PagedList<string>((query) => {
      const response = new Subject<PagedResult<string>>();
      requests.push({ query, response });
      return response;
    });
  });

  const latest = () => requests[requests.length - 1];

  it('asks for the first page and shows the rows when they arrive', () => {
    list.load();
    expect(latest().query).toEqual({ page: 1, pageSize: 10, search: '' });
    expect(list.loading()).toBe(true);

    latest().response.next(result(['a', 'b'], 1, 2));

    expect(list.items()).toEqual(['a', 'b']);
    expect(list.total()).toBe(2);
    expect(list.loading()).toBe(false);
  });

  it('setPage asks for that page', () => {
    list.setPage(3);

    expect(latest().query.page).toBe(3);
    expect(list.page()).toBe(3);
  });

  it('a new search goes back to page 1; the same search does nothing', () => {
    list.setPage(4);
    latest().response.next(result(['x'], 4, 40));

    list.setSearch('ข่าว');
    expect(latest().query).toEqual({ page: 1, pageSize: 10, search: 'ข่าว' });

    const before = requests.length;
    list.setSearch('ข่าว');
    expect(requests.length).toBe(before);
  });

  it('only the newest request counts: a slow old answer cannot overwrite a newer one', () => {
    list.setSearch('a');
    const slow = latest();
    list.setSearch('ab');
    const fast = latest();

    fast.response.next(result(['ab-result'], 1, 1));
    slow.response.next(result(['a-result'], 1, 1));

    expect(list.items()).toEqual(['ab-result']);
  });

  it('shows a message when loading fails, and reload tries again', () => {
    list.load();
    latest().response.error(new HttpErrorResponse({ status: 500 }));

    expect(list.error()).toContain('ระบบขัดข้อง');
    expect(list.loading()).toBe(false);

    list.reload();
    expect(list.error()).toBeNull();
    latest().response.next(result(['ok'], 1, 1));
    expect(list.items()).toEqual(['ok']);
  });

  it('steps back to the last page when the current page has become empty (its last row was deleted)', () => {
    list.setPage(3);
    latest().response.next(result([], 3, 20));

    expect(list.page()).toBe(2);
    expect(latest().query.page).toBe(2);

    latest().response.next(result(['row'], 2, 20));
    expect(list.items()).toEqual(['row']);
  });

  it('page 1 with no rows is just an empty list, not a loop', () => {
    list.load();
    const before = requests.length;

    latest().response.next(result([], 1, 0));

    expect(requests.length).toBe(before);
    expect(list.items()).toEqual([]);
    expect(list.loading()).toBe(false);
  });

  it('destroy stops listening to a request that is still running', () => {
    list.load();
    const pending = latest();

    list.destroy();
    pending.response.next(result(['late'], 1, 1));

    expect(list.items()).toEqual([]);
  });
});

describe('describeHttpError', () => {
  const message = (status: number, body?: unknown) => describeHttpError(new HttpErrorResponse({ status, error: body }));

  it('explains the usual failures in Thai', () => {
    expect(message(0)).toContain('เชื่อมต่อระบบไม่ได้');
    expect(message(401)).toContain('ไม่มีสิทธิ์');
    expect(message(403)).toContain('ไม่มีสิทธิ์');
    expect(message(404)).toContain('ไม่พบข้อมูล');
    expect(message(429)).toContain('ถี่เกินไป');
    expect(message(500)).toContain('ระบบขัดข้อง');
    expect(message(418)).toBe('ดำเนินการไม่สำเร็จ');
  });

  it('for a conflict, shows what the API said', () => {
    expect(message(409, { detail: 'File is still in use.' })).toBe('File is still in use.');
    expect(message(409)).toContain('ขัดแย้ง');
  });

  it('copes with something that is not an HTTP error', () => {
    expect(describeHttpError(new Error('boom'))).toContain('เกิดข้อผิดพลาด');
  });
});
