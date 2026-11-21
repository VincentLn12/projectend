import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { FormGroup } from '@angular/forms';
import { provideRouter } from '@angular/router';
import { ProjectEdit } from './project-edit/project-edit';

const STUDENT = '11111111-1111-1111-1111-111111111111';
const LECTURER_A = '22222222-2222-2222-2222-222222222222';
const LECTURER_B = '33333333-3333-3333-3333-333333333333';

interface EditPage {
  form: FormGroup;
  save(): void;
}

describe('ProjectEdit (new)', () => {
  let http: HttpTestingController;
  let page: EditPage;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{ path: '**', children: [] }])],
    });
    http = TestBed.inject(HttpTestingController);

    const fixture = TestBed.createComponent(ProjectEdit);
    fixture.detectChanges();
    http.expectOne((r) => r.url === '/api/admin/users').flush({
      items: [{ id: STUDENT, fullName: 'สมชาย', studentCode: '6412345' }],
      total: 1,
      page: 1,
      pageSize: 100,
    });
    http.expectOne((r) => r.url === '/api/admin/lecturers').flush({
      items: [
        { id: LECTURER_A, prefix: 'ดร.', fullName: 'อาจารย์ เอ' },
        { id: LECTURER_B, prefix: null, fullName: 'อาจารย์ บี' },
      ],
      total: 2,
      page: 1,
      pageSize: 100,
    });
    page = fixture.componentInstance as unknown as EditPage;
  });

  it('asks for the student and the Thai name before sending', () => {
    page.save();
    http.expectNone('/api/admin/projects');
    expect(page.form.get('studentId')?.touched).toBe(true);
    expect(page.form.get('nameTH')?.touched).toBe(true);
  });

  it('sends advisors with their roles, empty links as null, and the default Buddhist year', () => {
    page.form.patchValue({ nameTH: ' ระบบทดสอบ ', studentId: STUDENT, mainAdvisorId: LECTURER_A, coAdvisorId: LECTURER_B, webUrl: '' });
    page.save();

    const request = http.expectOne('/api/admin/projects');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toMatchObject({
      nameTH: 'ระบบทดสอบ',
      webUrl: null,
      studentId: STUDENT,
      academicYear: new Date().getFullYear() + 543,
      advisors: [
        { lecturerId: LECTURER_A, role: 1 },
        { lecturerId: LECTURER_B, role: 2 },
      ],
    });
    request.flush({});
  });

  it('refuses the same lecturer as main and co-advisor', () => {
    page.form.patchValue({ nameTH: 'x', studentId: STUDENT, mainAdvisorId: LECTURER_A, coAdvisorId: LECTURER_A });
    page.save();

    http.expectNone('/api/admin/projects');
    expect(page.form.get('coAdvisorId')?.errors?.['server']).toContain('คนละคน');
  });

  it('parses keyword tags and resets unsaved changes back to defaults', () => {
    const detailed = page as unknown as {
      form: FormGroup;
      keywordTags(): string[];
      resetForm(): void;
    };

    detailed.form.patchValue({ nameTH: 'โครงงานใหม่', keyWords: ' AI , Angular, , Web ' });
    expect(detailed.keywordTags()).toEqual(['AI', 'Angular', 'Web']);

    detailed.resetForm();
    expect(detailed.form.get('nameTH')?.value).toBe('');
    expect(detailed.keywordTags()).toEqual([]);
  });
});
