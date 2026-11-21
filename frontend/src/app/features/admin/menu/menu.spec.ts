import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Type } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { FALLBACK_MENU, MenuStore } from '../../../core/menu/api/menu.store';
import { MenuEdit } from './menu-edit/menu-edit';
import { MenuList } from './menu-list/menu-list';
import { AdminMenuItem } from './types/menu.types';

function item(id: string, label: string, sortOrder: number, parentId: string | null = null, kind = 1): AdminMenuItem {
  return { id, label, kind, target: kind === 1 ? '/news' : null, parentId, sortOrder, isVisible: true, openInNewTab: false };
}

function open<T>(component: Type<T>, params: Record<string, string> = {}) {
  TestBed.configureTestingModule({
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideRouter([{ path: '**', children: [] }]),
      { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap(params) } } },
    ],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(component);
  fixture.detectChanges();
  return { http, fixture };
}

describe('MenuEdit (new)', () => {
  function openNew() {
    const { http, fixture } = open(MenuEdit);
    http.expectOne('/api/admin/menu/pages').flush([{ path: '/news', label: 'ข่าว' }]);
    http.expectOne('/api/admin/menu').flush([item('g1', 'ข้อมูล', 0, null, 3)]);
    const page = fixture.componentInstance as unknown as { form: FormGroup; save(): void; formError(): string | null };
    return { http, page };
  }

  it('needs a page to be chosen for a page link', () => {
    const { http, page } = openNew();
    page.form.patchValue({ label: 'ข่าว', kind: 1 });
    page.save();

    http.expectNone('/api/admin/menu');
    expect(page.form.get('page')?.errors?.['server']).toBeTruthy();
  });

  it('refuses an external link that does not start with http or https', () => {
    const { http, page } = openNew();
    page.form.patchValue({ label: 'มหาวิทยาลัย', kind: 2, url: 'javascript:alert(1)' });
    page.save();

    http.expectNone('/api/admin/menu');
    expect(page.form.get('url')?.errors?.['server']).toContain('http');
  });

  it('sends only what the chosen kind uses', () => {
    const { http, page } = openNew();
    page.form.patchValue({ label: ' ข่าว ', kind: 1, page: '/news', url: 'https://ignored.example', parentId: 'g1', openInNewTab: true });
    page.save();

    const request = http.expectOne('/api/admin/menu');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      label: 'ข่าว',
      kind: 1,
      target: '/news',
      parentId: 'g1',
      isVisible: true,
      openInNewTab: true,
    });
    request.flush({});
  });

  it('sends a group with no link, no parent and no new-tab flag', () => {
    const { http, page } = openNew();
    page.form.patchValue({ label: 'ข้อมูล', kind: 3, page: '/news', parentId: 'g1', openInNewTab: true });
    page.save();

    const request = http.expectOne('/api/admin/menu');
    expect(request.request.body).toMatchObject({ kind: 3, target: null, parentId: null, openInNewTab: false });
    request.flush({});
  });

  it('shows the API message in Thai', () => {
    const { http, page } = openNew();
    page.form.patchValue({ label: 'ข้อมูล', kind: 2, url: 'https://example.com' });
    page.save();

    http.expectOne('/api/admin/menu').flush(
      { detail: 'External link must be a full http or https address.' },
      { status: 400, statusText: 'Bad Request' },
    );
    expect(page.formError()).toContain('http');
  });
});

describe('MenuList', () => {
  const items = [
    item('a', 'หน้าแรก', 0),
    item('g', 'ข้อมูล', 1, null, 3),
    item('c1', 'ข่าว', 0, 'g'),
    item('c2', 'โครงงาน', 1, 'g'),
    item('b', 'อาจารย์', 2),
  ];

  function openList() {
    const { http, fixture } = open(MenuList);
    http.expectOne('/api/admin/menu').flush(items);
    fixture.detectChanges();
    return {
      http,
      list: fixture.componentInstance as unknown as {
        rows(): { item: AdminMenuItem; depth: number; canMoveUp: boolean; canMoveDown: boolean }[];
        move(row: unknown, direction: -1 | 1): void;
      },
    };
  }

  it('lists each sub-menu item right under its group, and limits the arrows to its own level', () => {
    const { list } = openList();
    const rows = list.rows();

    expect(rows.map((row) => row.item.id)).toEqual(['a', 'g', 'c1', 'c2', 'b']);
    expect(rows.map((row) => row.depth)).toEqual([0, 0, 1, 1, 0]);
    expect(rows[0].canMoveUp).toBe(false);
    expect(rows[4].canMoveDown).toBe(false);
    expect(rows[2].canMoveUp).toBe(false);
    expect(rows[3].canMoveDown).toBe(false);
  });

  it('moving a sub-menu item down reorders only its group', () => {
    const { http, list } = openList();
    list.move(list.rows()[2], 1);

    const request = http.expectOne('/api/admin/menu/order');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ parentId: 'g', orderedIds: ['c2', 'c1'] });
    request.flush(null);
    http.expectOne('/api/admin/menu').flush(items);
  });
});

describe('MenuStore', () => {
  it('starts with the default menu so the bar is never empty, then shows what the API returns', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const http = TestBed.inject(HttpTestingController);
    const store = TestBed.inject(MenuStore);

    expect(store.menu()).toBe(FALLBACK_MENU);
    store.load();
    store.load();

    const request = http.expectOne('/api/menu');
    request.flush([{ id: 'x', label: 'เมนูใหม่', kind: 1, target: '/news', openInNewTab: false, children: [] }]);

    expect(store.menu().map((entry) => entry.label)).toEqual(['เมนูใหม่']);
  });

  it('keeps the default menu when the API is down', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const http = TestBed.inject(HttpTestingController);
    const store = TestBed.inject(MenuStore);

    store.load();
    http.expectOne('/api/menu').flush({}, { status: 503, statusText: 'Service Unavailable' });

    expect(store.menu()).toBe(FALLBACK_MENU);
  });
});
