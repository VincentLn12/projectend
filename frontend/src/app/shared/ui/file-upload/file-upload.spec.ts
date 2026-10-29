import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable, of, throwError } from 'rxjs';
import { FilesApi } from '../../../core/api/files.api';
import { FileDto } from '../../../core/models/file.models';
import { notEmptyListValidator } from '../../forms/validators';
import { FileUpload } from './file-upload';

class FakeFilesApi {
  uploaded: string[] = [];
  failWith: HttpErrorResponse | null = null;

  upload(file: File): Observable<FileDto> {
    if (this.failWith) {
      return throwError(() => this.failWith);
    }
    this.uploaded.push(file.name);
    return of({
      id: `id-${this.uploaded.length}`,
      originalName: file.name,
      contentType: file.type,
      sizeBytes: file.size,
      url: '',
      uploadedAt: '',
    });
  }

  url(id: string): string {
    return `/api/files/${id}`;
  }
}

@Component({
  imports: [ReactiveFormsModule, FileUpload],
  template: `
    <app-file-upload label="รูปปก" [formControl]="single" accept="image" />
    <app-file-upload label="แกลเลอรี" [formControl]="gallery" [multiple]="true" accept="any" />
  `,
})
class UploadHost {
  single = new FormControl<string | null>(null, Validators.required);
  gallery = new FormControl<string[]>([], notEmptyListValidator);
}

describe('FileUpload', () => {
  let api: FakeFilesApi;
  let fixture: ComponentFixture<UploadHost>;

  const picker = (index: number) => fixture.nativeElement.querySelectorAll('input[type="file"]')[index] as HTMLInputElement;

  const makeFile = (name: string, type: string, size = 1000) => {
    const file = new File(['x'], name, { type });
    Object.defineProperty(file, 'size', { value: size });
    return file;
  };

  async function choose(index: number, ...files: File[]): Promise<void> {
    Object.defineProperty(picker(index), 'files', { value: files, configurable: true });
    picker(index).dispatchEvent(new Event('change'));
    await fixture.whenStable();
    fixture.detectChanges();
    await fixture.whenStable();
  }

  beforeEach(async () => {
    api = new FakeFilesApi();
    TestBed.configureTestingModule({ providers: [{ provide: FilesApi, useValue: api }] });
    fixture = TestBed.createComponent(UploadHost);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('single mode: uploads the chosen picture and keeps its id; the choose button gives way to the picture', async () => {
    await choose(0, makeFile('cover.png', 'image/png'));

    expect(api.uploaded).toEqual(['cover.png']);
    expect(fixture.componentInstance.single.value).toBe('id-1');
    expect(fixture.nativeElement.querySelector('img')?.getAttribute('src')).toBe('/api/files/id-1');
    expect(fixture.nativeElement.querySelectorAll('input[type="file"]').length).toBe(1); // only the gallery's picker is left
  });

  it('single mode: removing the picture clears the value', async () => {
    await choose(0, makeFile('cover.png', 'image/png'));

    (Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[]).find((b) => b.textContent?.includes('ลบ'))?.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.single.value).toBeNull();
  });

  it('multiple mode: adds files to the list in the order chosen, first one is the main picture', async () => {
    await choose(1, makeFile('a.jpg', 'image/jpeg'), makeFile('b.pdf', 'application/pdf'));

    expect(fixture.componentInstance.gallery.value).toEqual(['id-1', 'id-2']);
    expect(fixture.nativeElement.textContent).toContain('รูปหลัก');
    // the PDF shows its name as a link, not as a picture
    expect(fixture.nativeElement.textContent).toContain('b.pdf');

    await choose(1, makeFile('c.png', 'image/png'));
    expect(fixture.componentInstance.gallery.value).toEqual(['id-1', 'id-2', 'id-3']);
  });

  it('multiple mode: "make main" moves a picture to the front and remove takes it out', async () => {
    await choose(1, makeFile('a.jpg', 'image/jpeg'), makeFile('b.jpg', 'image/jpeg'), makeFile('c.jpg', 'image/jpeg'));
    const buttons = () => Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[];

    buttons().filter((b) => b.textContent?.includes('ทำเป็นรูปหลัก'))[1].click(); // the third picture
    fixture.detectChanges();
    expect(fixture.componentInstance.gallery.value).toEqual(['id-3', 'id-1', 'id-2']);

    buttons().filter((b) => b.textContent?.includes('ลบ'))[0].click(); // the new first one
    fixture.detectChanges();
    expect(fixture.componentInstance.gallery.value).toEqual(['id-1', 'id-2']);
  });

  it('refuses a file type the API does not accept, without uploading', async () => {
    await choose(0, makeFile('notes.txt', 'text/plain'));

    expect(api.uploaded).toEqual([]);
    expect(fixture.componentInstance.single.value).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('ไม่ใช่ชนิดที่รองรับ');
  });

  it('refuses a file over 10 MB', async () => {
    await choose(0, makeFile('big.png', 'image/png', 11 * 1024 * 1024));

    expect(api.uploaded).toEqual([]);
    expect(fixture.nativeElement.textContent).toContain('ใหญ่เกิน 10 MB');
  });

  it('a PDF is refused where only pictures are accepted', async () => {
    await choose(0, makeFile('doc.pdf', 'application/pdf'));

    expect(api.uploaded).toEqual([]);
  });

  it('shows the API message when the upload is rejected', async () => {
    api.failWith = new HttpErrorResponse({ status: 400, error: { detail: 'File extension does not match its content.' } });

    await choose(0, makeFile('fake.png', 'image/png'));

    expect(fixture.componentInstance.single.value).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('File extension does not match its content.');
  });

  it('asks the person to sign in again when the API says 401', async () => {
    api.failWith = new HttpErrorResponse({ status: 401 });

    await choose(0, makeFile('cover.png', 'image/png'));

    expect(fixture.nativeElement.textContent).toContain('เข้าสู่ระบบอีกครั้ง');
  });

  it('shows saved ids it knows nothing about, and falls back to a link when the picture will not load', async () => {
    fixture.componentInstance.gallery.setValue(['old-id']);
    fixture.detectChanges();
    await fixture.whenStable();

    const image = fixture.nativeElement.querySelector('img') as HTMLImageElement;
    expect(image.getAttribute('src')).toBe('/api/files/old-id');

    image.dispatchEvent(new Event('error'));
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('img')).toBeNull();
    expect((fixture.nativeElement.querySelector('a[href="/api/files/old-id"]') as HTMLAnchorElement).textContent).toContain('เปิดไฟล์');
  });

  it('a required empty field shows its error after the person touches it', async () => {
    fixture.componentInstance.single.markAsTouched();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain('กรุณากรอกข้อมูล');
  });
});
