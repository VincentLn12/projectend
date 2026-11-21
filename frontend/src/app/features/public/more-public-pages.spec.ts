import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { PublicSyllabus } from './syllabus/syllabus-page/syllabus-page';

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
