import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ConfirmService } from '../../shared/ui/confirm-dialog/confirm.service';
import { describeRegistrationError } from './courses/api/courses.api';
import { PublicCourseDetail } from './courses/course-detail/course-detail';
import { PublicSyllabus } from './syllabus/syllabus-page/syllabus-page';

describe('describeRegistrationError', () => {
  const failure = (status: number, detail: string) => new HttpErrorResponse({ status, error: { detail } });

  it('turns the API messages into Thai', () => {
    expect(describeRegistrationError(failure(409, 'This generation is full.'))).toBe('รุ่นนี้เต็มแล้ว');
    expect(describeRegistrationError(failure(409, 'You are already registered for this generation.'))).toBe(
      'คุณลงทะเบียนรุ่นนี้ไว้แล้ว',
    );
    expect(describeRegistrationError(failure(400, 'This generation has been cancelled.'))).toBe('รุ่นนี้ถูกยกเลิกแล้ว');
    expect(describeRegistrationError(failure(400, 'Registration is closed because the generation has already started.'))).toContain(
      'ปิดรับสมัคร',
    );
  });

  it('falls back to the general message for anything else', () => {
    expect(describeRegistrationError(new HttpErrorResponse({ status: 0 }))).toContain('เชื่อมต่อระบบไม่ได้');
  });
});

describe('PublicSyllabus', () => {
  it('groups the subjects by category with their credit totals, in category order', () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({})) } },
      ],
    });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(PublicSyllabus);
    fixture.detectChanges();

    http.expectOne('/api/syllabuses').flush([]);
    http.expectOne('/api/syllabuses/current').flush({
      id: 's1',
      nameTH: 'หลักสูตรทดสอบ',
      year: 2569,
      isCurrent: true,
      objectives: [],
      occupations: [],
      subjects: [
        { id: 'a', code: null, name: 'เลือกเสรี', credit: 6, subjectCategory: 2 },
        { id: 'b', code: null, name: 'ภาษา', credit: 12, subjectCategory: 0 },
        { id: 'c', code: null, name: 'มนุษย์', credit: 6, subjectCategory: 0 },
      ],
    });
    fixture.detectChanges();

    const groups = (fixture.componentInstance as unknown as { groups(): { label: string; totalCredits: number }[] }).groups();
    expect(groups.map((group) => [group.label, group.totalCredits])).toEqual([
      ['หมวดวิชาศึกษาทั่วไป', 18],
      ['หมวดวิชาเลือกเสรี', 6],
    ]);
  });
});

describe('PublicCourseDetail', () => {
  const generation = {
    id: 'g1',
    courseId: 'c1',
    subTitle: 'รุ่น 1',
    description: null,
    startDate: '2030-01-01T00:00:00Z',
    endDate: '2030-01-03T00:00:00Z',
    quantity: 10,
    registeredCount: 2,
    fee: null,
    photoFileId: null,
    isCancelled: false,
    isRegistrationOpen: true,
  };
  const course = { id: 'c1', title: 'อบรม', description: null, lecturerName: null, imageFileIds: [], generations: [generation] };

  function open(loggedIn: boolean) {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: '**', children: [] }]),
        { provide: AuthService, useValue: { isLoggedIn: signal(loggedIn) } },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: 'c1' }) } } },
      ],
    });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(PublicCourseDetail);
    fixture.detectChanges();
    return { http, fixture };
  }

  const buttonTexts = (root: HTMLElement) =>
    Array.from(root.querySelectorAll('button')).map((button) => button.textContent?.trim());

  it('asks a visitor to sign in instead of offering to register', () => {
    const { http, fixture } = open(false);
    http.expectOne('/api/courses/c1').flush(course);
    fixture.detectChanges();

    expect(buttonTexts(fixture.nativeElement)).toEqual(['เข้าสู่ระบบเพื่อลงทะเบียน']);
    expect(fixture.nativeElement.textContent).toContain('ที่นั่งว่าง 8 จาก 10');
    http.expectNone('/api/me/registrations');
  });

  it('lets a signed-in person register, and shows "registered" with a cancel button afterwards', () => {
    const { http, fixture } = open(true);
    http.expectOne('/api/me/registrations').flush([]);
    http.expectOne('/api/courses/c1').flush(course);
    fixture.detectChanges();
    expect(buttonTexts(fixture.nativeElement)).toEqual(['ลงทะเบียน']);

    (fixture.nativeElement.querySelector('button') as HTMLButtonElement).click();
    const post = http.expectOne('/api/generations/g1/register');
    expect(post.request.method).toBe('POST');
    post.flush({});

    http.expectOne('/api/me/registrations').flush([{ generationId: 'g1', courseId: 'c1', status: 2 }]);
    http.expectOne('/api/courses/c1').flush({ ...course, generations: [{ ...generation, registeredCount: 3 }] });
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('ลงทะเบียนแล้ว');
    expect(buttonTexts(fixture.nativeElement)).toEqual(['ยกเลิก']);
  });

  it('does not cancel unless the person confirms', async () => {
    const { http, fixture } = open(true);
    http.expectOne('/api/me/registrations').flush([{ generationId: 'g1', courseId: 'c1', status: 2 }]);
    http.expectOne('/api/courses/c1').flush(course);
    fixture.detectChanges();

    const confirm = TestBed.inject(ConfirmService);
    (fixture.nativeElement.querySelector('button') as HTMLButtonElement).click();
    expect(confirm.current()?.options.danger).toBe(true);
    confirm.answer(false);
    await fixture.whenStable();

    http.expectNone('/api/generations/g1/register');
  });
});
