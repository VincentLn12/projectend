import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { FormGroup } from '@angular/forms';
import { provideRouter } from '@angular/router';
import { LecturerEdit } from './lecturer-edit/lecturer-edit';

interface EditPage {
  form: FormGroup;
  save(): void;
}

describe('LecturerEdit (new)', () => {
  let http: HttpTestingController;
  let page: EditPage;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{ path: '**', children: [] }])],
    });
    http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(LecturerEdit);
    fixture.detectChanges();
    page = fixture.componentInstance as unknown as EditPage;
  });

  it('needs a name before sending', () => {
    page.save();
    http.expectNone('/api/admin/lecturers');
    expect(page.form.get('fullName')?.touched).toBe(true);
  });

  it('sends trimmed text with empty fields as null', () => {
    page.form.patchValue({ prefix: ' ดร. ', fullName: ' สมหญิง ', position: '', sortOrder: 3, isPublished: true });
    page.save();

    const request = http.expectOne('/api/admin/lecturers');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toMatchObject({
      prefix: 'ดร.',
      fullName: 'สมหญิง',
      position: null,
      contact: null,
      sortOrder: 3,
      isPublished: true,
    });
    request.flush({});
  });
});
