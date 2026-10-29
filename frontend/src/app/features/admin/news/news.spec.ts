import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ConfirmService } from '../../../shared/ui/confirm-dialog/confirm.service';
import { AdminNewsApi } from './api/news.api';
import { NewsEdit } from './news-edit/news-edit';
import { NewsList } from './news-list/news-list';
import { AdminNews } from './types/news.types';

const FILE_A = '11111111-1111-1111-1111-111111111111';
const FILE_B = '22222222-2222-2222-2222-222222222222';

function news(overrides: Partial<AdminNews> = {}): AdminNews {
  return {
    id: 'n1',
    title: 'ข่าวทดสอบ',
    subTitle: null,
    body: 'เนื้อหา',
    publishedAt: null,
    isPublished: false,
    authorName: 'admin',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    files: [],
    ...overrides,
  };
}

describe('admin news', () => {
  let http: HttpTestingController;

  function setup() {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{ path: '**', children: [] }])],
    });
    http = TestBed.inject(HttpTestingController);
  }

  describe('AdminNewsApi', () => {
    beforeEach(() => setup());

    it('lists with paging and sends the search only when there is one', () => {
      const api = TestBed.inject(AdminNewsApi);

      api.list({ page: 2, pageSize: 10, search: '' }).subscribe();
      const plain = http.expectOne((r) => r.url === '/api/admin/news');
      expect(plain.request.params.get('page')).toBe('2');
      expect(plain.request.params.has('search')).toBe(false);
      plain.flush({ items: [], total: 0, page: 2, pageSize: 10 });

      api.list({ page: 1, pageSize: 10, search: 'งาน' }).subscribe();
      const searched = http.expectOne((r) => r.url === '/api/admin/news');
      expect(searched.request.params.get('search')).toBe('งาน');
      searched.flush({ items: [], total: 0, page: 1, pageSize: 10 });
    });

    it('sends the sort order and the filters as query parameters', () => {
      TestBed.inject(AdminNewsApi)
        .list({
          page: 1,
          pageSize: 10,
          search: '',
          sort: { key: 'publishedAt', direction: 'desc' },
          filters: { isPublished: 'true', publishedFrom: '2026-10-01', publishedTo: '2026-10-31' },
        })
        .subscribe();

      const request = http.expectOne((r) => r.url === '/api/admin/news');
      expect(request.request.params.get('sortBy')).toBe('publishedAt');
      expect(request.request.params.get('sortDescending')).toBe('true');
      expect(request.request.params.get('isPublished')).toBe('true');
      expect(request.request.params.get('publishedFrom')).toBe('2026-10-01');
      expect(request.request.params.get('publishedTo')).toBe('2026-10-31');
      request.flush({ items: [], total: 0, page: 1, pageSize: 10 });
    });

    it('publishes through the PATCH endpoint', () => {
      TestBed.inject(AdminNewsApi).setPublished('n1', true).subscribe();
      const request = http.expectOne('/api/admin/news/n1/published');
      expect(request.request.method).toBe('PATCH');
      expect(request.request.body).toEqual({ isPublished: true });
      request.flush(news({ isPublished: true }));
    });
  });

  describe('NewsList', () => {
    beforeEach(() => setup());

    it('asks the API again when the status or a date is chosen, and clears them together', () => {
      const fixture = TestBed.createComponent(NewsList);
      fixture.detectChanges();
      http.expectOne((r) => r.url === '/api/admin/news').flush({ items: [], total: 0, page: 1, pageSize: 10 });

      const page = fixture.componentInstance as unknown as {
        statusControl: import('@angular/forms').FormControl<boolean | null>;
        publishedFromControl: import('@angular/forms').FormControl<string | null>;
        clearFilters(): void;
      };

      page.statusControl.setValue(false);
      const byStatus = http.expectOne((r) => r.url === '/api/admin/news');
      expect(byStatus.request.params.get('isPublished')).toBe('false');
      byStatus.flush({ items: [], total: 0, page: 1, pageSize: 10 });

      page.publishedFromControl.setValue('2026-10-01');
      const byDate = http.expectOne((r) => r.url === '/api/admin/news');
      expect(byDate.request.params.get('isPublished')).toBe('false');
      expect(byDate.request.params.get('publishedFrom')).toBe('2026-10-01');
      byDate.flush({ items: [], total: 0, page: 1, pageSize: 10 });

      page.clearFilters();
      const requests = http.match((r) => r.url === '/api/admin/news');
      const last = requests[requests.length - 1];
      expect(last.request.params.has('isPublished')).toBe(false);
      expect(last.request.params.has('publishedFrom')).toBe(false);
      last.flush({ items: [], total: 0, page: 1, pageSize: 10 });
    });

    it('does not delete when the confirmation is declined, and deletes when accepted', async () => {
      const fixture = TestBed.createComponent(NewsList);
      fixture.detectChanges();
      http.expectOne((r) => r.url === '/api/admin/news').flush({ items: [], total: 0, page: 1, pageSize: 10 });

      const confirm = TestBed.inject(ConfirmService);
      const row = { id: 'n1', title: 'ข่าว', publishedAt: null, isPublished: false, authorName: 'a', createdAt: '' };

      const declined = (fixture.componentInstance as unknown as { askToDelete(r: typeof row): Promise<void> }).askToDelete(row);
      expect(confirm.current()?.options.danger).toBe(true);
      confirm.answer(false);
      await declined;
      http.expectNone('/api/admin/news/n1');

      const accepted = (fixture.componentInstance as unknown as { askToDelete(r: typeof row): Promise<void> }).askToDelete(row);
      confirm.answer(true);
      await accepted;
      const request = http.expectOne('/api/admin/news/n1');
      expect(request.request.method).toBe('DELETE');
      request.flush(null);
      http.expectOne((r) => r.url === '/api/admin/news').flush({ items: [], total: 0, page: 1, pageSize: 10 });
    });
  });

  describe('NewsEdit (new)', () => {
    beforeEach(() => setup());

    function open() {
      const fixture = TestBed.createComponent(NewsEdit);
      fixture.detectChanges();
      return fixture.componentInstance as unknown as {
        form: import('@angular/forms').FormGroup;
        save(): void;
        formError(): string | null;
      };
    }

    it('does not send an invalid form', () => {
      const page = open();
      page.save();
      http.expectNone('/api/admin/news');
      expect(page.form.get('title')?.touched).toBe(true);
    });

    it('sends trimmed text, the pictures in order and the first one as the main picture', () => {
      const page = open();
      page.form.patchValue({ title: '  หัวข้อ  ', subTitle: '   ', body: 'เนื้อหา', isPublished: true, fileIds: [FILE_B, FILE_A] });
      page.save();

      const request = http.expectOne('/api/admin/news');
      expect(request.request.method).toBe('POST');
      expect(request.request.body).toEqual({
        title: 'หัวข้อ',
        subTitle: null,
        body: 'เนื้อหา',
        isPublished: true,
        fileIds: [FILE_B, FILE_A],
        mainFileId: FILE_B,
      });
      request.flush(news());
    });

    it('shows the API validation message under the field', () => {
      const page = open();
      page.form.patchValue({ title: 'x', body: 'y' });
      page.save();

      http.expectOne('/api/admin/news').flush(
        { errors: { Title: ['หัวข้อซ้ำ'] } },
        { status: 400, statusText: 'Bad Request' },
      );

      expect(page.form.get('title')?.errors).toEqual({ server: 'หัวข้อซ้ำ' });
      expect(page.formError()).toBeNull();
    });
  });
});
