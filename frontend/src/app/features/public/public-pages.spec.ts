import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ThaiDatePipe } from '../../shared/pipes/thai-date.pipe';
import { FileImage } from '../../shared/ui/file-image/file-image';
import { PublicProjectList } from './projects/project-list/project-list';

describe('ThaiDatePipe', () => {
  const pipe = new ThaiDatePipe();

  it('shows the Buddhist year and the Thai month', () => {
    expect(pipe.transform('2026-05-31T05:00:00Z')).toBe('31 พฤษภาคม 2569');
    expect(pipe.transform('2026-05-31T05:00:00Z', 'short')).toBe('31 พ.ค. 2569');
  });

  it('shows a dash for empty or invalid values', () => {
    expect(pipe.transform(null)).toBe('—');
    expect(pipe.transform('not a date')).toBe('—');
  });
});

@Component({ imports: [FileImage], template: '<app-file-image [fileId]="id" alt="ปก" />' })
class FileImageHost {
  id: string | null = 'abc';
}

describe('FileImage', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
  });

  it('points at the file URL, and falls back to a placeholder when the file cannot be loaded', () => {
    const fixture = TestBed.createComponent(FileImageHost);
    fixture.detectChanges();
    const image = fixture.nativeElement.querySelector('img') as HTMLImageElement;
    expect(image.getAttribute('src')).toBe('/api/files/abc');

    image.dispatchEvent(new Event('error'));
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('img')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('ไม่มีรูปภาพ');
  });

  it('shows the placeholder when there is no file at all', () => {
    const fixture = TestBed.createComponent(FileImageHost);
    fixture.componentInstance.id = null;
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('img')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('ไม่มีรูปภาพ');
  });
});

describe('PublicProjectList', () => {
  it('asks the API again from page 1 when the year changes', () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(PublicProjectList);
    fixture.detectChanges();

    const first = http.expectOne((r) => r.url === '/api/projects');
    expect(first.request.params.has('academicYear')).toBe(false);
    first.flush({ items: [], total: 0, page: 1, pageSize: 9 });

    (fixture.componentInstance as unknown as { year: { setValue(v: number): void } }).year.setValue(2568);

    const filtered = http.expectOne((r) => r.url === '/api/projects');
    expect(filtered.request.params.get('academicYear')).toBe('2568');
    expect(filtered.request.params.get('page')).toBe('1');
    filtered.flush({ items: [], total: 0, page: 1, pageSize: 9 });
  });
});
