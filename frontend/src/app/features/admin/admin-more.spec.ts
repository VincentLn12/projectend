import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Type } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormArray, FormGroup } from '@angular/forms';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { HomeBannerEdit } from './home-banners/home-banner-edit/home-banner-edit';
import { JobHistoryEdit } from './job-histories/job-history-edit/job-history-edit';
import { SettingsPage } from './settings/settings-page/settings-page';
import { SyllabusEdit } from './syllabuses/syllabus-edit/syllabus-edit';
import { TechSubjectEdit } from './tech-subjects/tech-subject-edit/tech-subject-edit';
import { describeUserError } from './users/api/users.api';
import { UserEdit } from './users/user-edit/user-edit';

interface FormPage {
  form: FormGroup;
  save(): void;
}

function open<T>(component: Type<T>, params: Record<string, string> = {}) {
  TestBed.configureTestingModule({
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideRouter([{ path: '**', children: [] }]),
      { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap(params), queryParamMap: convertToParamMap({}) } } },
    ],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(component);
  fixture.detectChanges();
  return { http, fixture, page: fixture.componentInstance as unknown as FormPage };
}

describe('generated admin pages', () => {
  it('tech subject: sends trimmed text, empty fields as null and the picture ids', () => {
    const { http, page } = open(TechSubjectEdit);
    page.form.patchValue({ subjectName: ' .NET ', icon: '', fileIds: ['f1', 'f2'], sortOrder: 2 });
    page.save();

    const request = http.expectOne('/api/admin/tech-subjects');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toMatchObject({ subjectName: '.NET', icon: null, subTitle: null, fileIds: ['f1', 'f2'], sortOrder: 2 });
    request.flush({});
  });

  it('banner: needs a picture, and rejects a link that is not a URL', () => {
    const { http, page } = open(HomeBannerEdit);
    page.form.patchValue({ linkUrl: 'not a url' });
    page.save();

    http.expectNone('/api/admin/home-banners');
    expect(page.form.get('fileId')?.errors?.['required']).toBeTruthy();
    expect(page.form.get('linkUrl')?.invalid).toBe(true);
  });

  it('job history: loads the people to choose from and sends the chosen one', () => {
    const { http, page } = open(JobHistoryEdit);
    http.expectOne((r) => r.url === '/api/admin/users').flush({
      items: [{ id: 'u1', fullName: 'สมชาย', studentCode: '6412345' }],
      total: 1,
      page: 1,
      pageSize: 100,
    });

    page.form.patchValue({ userId: 'u1', jobName: 'โปรแกรมเมอร์', startDate: '2024-06-01' });
    page.save();

    const request = http.expectOne('/api/admin/job-histories');
    expect(request.request.body).toMatchObject({ userId: 'u1', jobName: 'โปรแกรมเมอร์', startDate: '2024-06-01', company: null });
    request.flush({});
  });
});

describe('SettingsPage', () => {
  it('loads the current settings and saves empty links as null', () => {
    const { http, page } = open(SettingsPage);
    http.expectOne('/api/settings').flush({
      webName: 'เดิม',
      registerUrl: 'https://example.com',
      kruUrl: null,
      pageFacebook: null,
      logoFileId: null,
      videoUrl: null,
      mapLocation: null,
      quickMenuEnabled: true,
      quickMenuBackgroundFileId: null,
      quickMenuItems: [{ key: 'news', enabled: false, title: null, imageFileId: null }],
    });
    expect(page.form.get('webName')?.value).toBe('เดิม');

    page.form.patchValue({ webName: ' ชื่อใหม่ ', registerUrl: '' });
    page.save();

    const request = http.expectOne('/api/admin/settings');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toMatchObject({ webName: 'ชื่อใหม่', registerUrl: null, kruUrl: null, quickMenuEnabled: true });
    expect((request.request.body as { quickMenuItems: { key: string }[] }).quickMenuItems[0].key).toBe('news');
    request.flush(request.request.body);
  });
});

describe('UserEdit', () => {
  it('asks a student for the student code and the entry year', () => {
    const { http, page } = open(UserEdit);
    page.form.patchValue({ email: 'a@b.co', fullName: 'นักศึกษา', password: 'longenough1', userType: 2 });
    page.save();

    http.expectNone('/api/admin/users');
    expect(page.form.get('studentCode')?.errors?.['server']).toBeTruthy();
    expect(page.form.get('entryYear')?.errors?.['server']).toBeTruthy();
  });

  it('does not send student data for someone who is not a student', () => {
    const { http, page } = open(UserEdit);
    page.form.patchValue({ email: 'a@b.co', fullName: 'บุคคลทั่วไป', password: 'longenough1', userType: 1, studentCode: '999' });
    page.save();

    const request = http.expectOne('/api/admin/users');
    expect(request.request.body).toMatchObject({ email: 'a@b.co', userType: 1, studentCode: null, entryYear: null });
    request.flush({});
  });

  it('turns the API messages into Thai', () => {
    const failure = (detail: string) => new HttpErrorResponse({ status: 409, error: { detail } });
    expect(describeUserError(failure('This email is already used.'))).toBe('อีเมลนี้ถูกใช้แล้ว');
    expect(describeUserError(failure('You cannot delete your own account.'))).toBe('ไม่สามารถลบบัญชีของตัวเองได้');
    expect(describeUserError(failure('Cannot remove the last administrator.'))).toContain('ผู้ดูแลระบบคนสุดท้าย');
  });
});

describe('SyllabusEdit', () => {
  it('sends the objectives, occupations and subjects that were added', () => {
    const { http, page, fixture } = open(SyllabusEdit);
    const component = fixture.componentInstance as unknown as {
      addObjective(text: string): void;
      addOccupation(text: string): void;
      addSubject(code: string, name: string, credit: number, category: number): void;
      subjects: FormArray;
    };

    page.form.patchValue({ nameTH: 'หลักสูตร', year: 2569 });
    component.addObjective(' ข้อหนึ่ง ');
    component.addOccupation('โปรแกรมเมอร์');
    component.addSubject('', 'วิชาแรก', 3, 1);
    page.save();

    const request = http.expectOne('/api/admin/syllabuses');
    expect(request.request.body).toMatchObject({
      nameTH: 'หลักสูตร',
      year: 2569,
      objectives: ['ข้อหนึ่ง'],
      occupations: ['โปรแกรมเมอร์'],
      subjects: [{ code: null, name: 'วิชาแรก', credit: 3, subjectCategory: 1 }],
    });
    request.flush({});
  });

  it('does not send a subject without a name', () => {
    const { http, page, fixture } = open(SyllabusEdit);
    (fixture.componentInstance as unknown as { addSubject(a: string, b: string, c: number, d: number): void }).addSubject('', '', 3, 0);
    page.form.patchValue({ nameTH: 'หลักสูตร' });
    page.save();
    http.expectNone('/api/admin/syllabuses');
  });
});

describe('SyllabusEdit category PDFs', () => {
  const FILE_SPECIFIC = '11111111-1111-1111-1111-111111111111';
  const FILE_FREE = '22222222-2222-2222-2222-222222222222';

  it('sends one PDF per category that has a file, and none for empty slots', () => {
    const { http, page } = open(SyllabusEdit);
    page.form.patchValue({ nameTH: 'หลักสูตร', year: 2569, categoryPdf1: FILE_SPECIFIC, categoryPdf2: FILE_FREE });
    page.save();

    const request = http.expectOne('/api/admin/syllabuses');
    expect(request.request.body.categoryFiles).toEqual([
      { subjectCategory: 1, fileId: FILE_SPECIFIC },
      { subjectCategory: 2, fileId: FILE_FREE },
    ]);
    request.flush({});
  });

  it('sends an empty list when no category has a PDF', () => {
    const { http, page } = open(SyllabusEdit);
    page.form.patchValue({ nameTH: 'หลักสูตร', year: 2569 });
    page.save();

    const request = http.expectOne('/api/admin/syllabuses');
    expect(request.request.body.categoryFiles).toEqual([]);
    request.flush({});
  });

  it('fills each slot from the saved category files when editing', () => {
    const { http, page } = open(SyllabusEdit, { id: 's1' });
    http.expectOne('/api/admin/syllabuses/s1').flush({
      id: 's1', nameTH: 'หลักสูตร', nameEN: null, year: 2569, degreeTH: null, degreeEN: null, totalCredits: null,
      pdfFileId: null, isCurrent: false, isPublished: false, objectives: [], occupations: [], subjects: [],
      categoryFiles: [{ subjectCategory: 1, fileId: FILE_SPECIFIC }],
    });

    expect(page.form.get('categoryPdf1')?.value).toBe(FILE_SPECIFIC);
    expect(page.form.get('categoryPdf0')?.value).toBeNull();
  });
});
