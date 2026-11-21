import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { FormArray, FormGroup } from '@angular/forms';
import { provideRouter } from '@angular/router';
import { CertificateTemplateEdit } from './certificate-template-edit/certificate-template-edit';

interface EditPage {
  form: FormGroup;
  elements: FormArray<FormGroup>;
  save(): void;
  addElement(type: 'text' | 'image'): void;
  moveElement(index: number, step: -1 | 1): void;
  removeElement(index: number): void;
  duplicateElement(index: number): void;
  sampleText(text: string | null): string;
  fontSizeInContainerWidth(fontSize: number | null): number;
  hasUnsavedChanges(): boolean;
}

describe('CertificateTemplateEdit (new)', () => {
  let http: HttpTestingController;
  let page: EditPage;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{ path: '**', children: [] }])],
    });
    http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(CertificateTemplateEdit);
    fixture.detectChanges();
    page = fixture.componentInstance as unknown as EditPage;
  });

  it('starts from the standard certificate wording', () => {
    expect(page.elements.length).toBe(7);
    expect(page.elements.at(3).get('text')?.value).toBe('{ชื่อ-นามสกุล}');
  });

  it('needs a name before sending', () => {
    page.save();
    http.expectNone('/api/admin/certificate-templates');
    expect(page.form.get('name')?.touched).toBe(true);
  });

  it('sends page settings and every element', () => {
    page.form.patchValue({ name: ' แม่แบบอบรม ', pageSize: 'A5', landscape: false });
    page.save();

    const request = http.expectOne('/api/admin/certificate-templates');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toMatchObject({ name: 'แม่แบบอบรม', pageSize: 'A5', landscape: false, backgroundFileId: null });
    expect(request.request.body.elements.length).toBe(7);
    expect(request.request.body.elements[1]).toMatchObject({ type: 'text', text: 'เกียรติบัตร', fontSize: 50, bold: true, color: '#0f2854', align: 'center', imageFileId: null });
    request.flush({});
  });

  it('adds an image element that has no text and blocks a bad colour', () => {
    page.addElement('image');
    expect(page.elements.length).toBe(8);
    expect(page.elements.at(7).get('type')?.value).toBe('image');

    page.form.patchValue({ name: 'x' });
    page.elements.at(0).get('color')?.setValue('red');
    page.save();
    http.expectNone('/api/admin/certificate-templates');
  });

  it('reorders, duplicates and removes elements', () => {
    const firstText = page.elements.at(0).get('text')?.value;
    page.moveElement(0, 1);
    expect(page.elements.at(1).get('text')?.value).toBe(firstText);

    page.duplicateElement(1);
    expect(page.elements.length).toBe(8);

    page.removeElement(1);
    expect(page.elements.length).toBe(7);
  });

  it('shows sample data for the variables in the live preview', () => {
    expect(page.sampleText('ขอมอบให้ {ชื่อ-นามสกุล}')).toBe('ขอมอบให้ นายสมชาย ใจดี');
  });

  it('scales the font by the page width so the preview matches the PDF', () => {
    page.form.patchValue({ pageSize: 'A4', landscape: true });
    expect(page.fontSizeInContainerWidth(42.1)).toBeCloseTo(5, 1);
  });

  it('warns about unsaved edits only after a change', () => {
    expect(page.hasUnsavedChanges()).toBe(false);
    page.form.get('name')?.markAsDirty();
    expect(page.hasUnsavedChanges()).toBe(true);
  });
});
