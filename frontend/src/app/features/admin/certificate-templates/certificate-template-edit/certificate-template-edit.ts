import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { startWith } from 'rxjs';
import { saveBlobAsFile } from '../../../../shared/data/download-file';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { BackLink } from '../../../../shared/navigation/back-link';
import { Button } from '../../../../shared/ui/button/button';
import { Checkbox } from '../../../../shared/ui/checkbox/checkbox';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { AdminCertificateTemplatesApi } from '../api/certificate-templates.api';
import {
  CertificateAlign,
  CertificateElement,
  CertificatePageSize,
  CertificateTemplate,
  PAGE_SIZE_POINTS,
  SaveCertificateTemplateRequest,
  TEMPLATE_VARIABLES,
} from '../types/certificate-template.types';

const COLOR_PATTERN = /^#[0-9a-fA-F]{6}$/;

type ElementGroup = FormGroup<{
  type: FormControl<'text' | 'image'>;
  x: FormControl<number | null>;
  y: FormControl<number | null>;
  width: FormControl<number | null>;
  height: FormControl<number | null>;
  text: FormControl<string>;
  fontSize: FormControl<number | null>;
  bold: FormControl<boolean>;
  italic: FormControl<boolean>;
  underline: FormControl<boolean>;
  color: FormControl<string>;
  align: FormControl<CertificateAlign>;
  imageFileId: FormControl<string | null>;
}>;

function textElement(text: string, y: number, height: number, fontSize: number, bold: boolean, color: string): CertificateElement {
  return { type: 'text', x: 8, y, width: 84, height, text, fontSize, bold, italic: false, underline: false, color, align: 'center', imageFileId: null };
}

// A new template starts from the same wording the fixed certificate used, so it is never blank.
function standardElements(): CertificateElement[] {
  return [
    textElement('สาขาวิชาวิทยาการคอมพิวเตอร์ มหาวิทยาลัยราชภัฏกาญจนบุรี', 9, 7, 24, true, '#000000'),
    textElement('เกียรติบัตร', 17, 14, 50, true, '#0f2854'),
    textElement('ขอมอบเกียรติบัตรฉบับนี้ให้ไว้เพื่อแสดงว่า', 33, 8, 26, false, '#000000'),
    textElement('{ชื่อ-นามสกุล}', 41, 10, 38, true, '#1c5bb8'),
    textElement('ได้เข้าร่วมการอบรมหลักสูตร', 52, 8, 26, false, '#000000'),
    textElement('{ชื่อหลักสูตร}', 59, 9, 30, true, '#000000'),
    textElement('เมื่อวันที่ {วันที่อบรม} {จำนวนชั่วโมง}', 68, 8, 24, false, '#000000'),
  ];
}

@Component({
  selector: 'app-certificate-template-edit',
  imports: [ReactiveFormsModule, BackLink, Button, Checkbox, DataState, FileImage, FileUpload, SelectInput, TextArea, TextInput, Toggle],
  templateUrl: './certificate-template-edit.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CertificateTemplateEdit implements OnInit {
  private readonly api = inject(AdminCertificateTemplatesApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly templateId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.templateId === null;

  protected readonly form = this.formBuilder.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    pageSize: ['A4' as CertificatePageSize],
    landscape: [true],
    backgroundFileId: [null as string | null],
    elements: new FormArray<ElementGroup>([]),
  });

  protected get elements(): FormArray<ElementGroup> {
    return this.form.controls.elements;
  }

  protected readonly pageSizeOptions: SelectOption<CertificatePageSize>[] = [
    { value: 'A4', label: 'A4' },
    { value: 'A5', label: 'A5' },
    { value: 'Letter', label: 'Letter' },
  ];
  protected readonly alignOptions: SelectOption<CertificateAlign>[] = [
    { value: 'left', label: 'ชิดซ้าย' },
    { value: 'center', label: 'กึ่งกลาง' },
    { value: 'right', label: 'ชิดขวา' },
  ];
  protected readonly variables = TEMPLATE_VARIABLES;

  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly previewing = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly selectedIndex = signal<number | null>(null);

  private readonly formValue = toSignal(this.form.valueChanges.pipe(startWith(null)), { initialValue: null });

  protected readonly aspectRatio = computed(() => {
    this.formValue();
    const size = PAGE_SIZE_POINTS[this.form.controls.pageSize.value ?? 'A4'];
    return this.form.controls.landscape.value ? `${size.height} / ${size.width}` : `${size.width} / ${size.height}`;
  });

  protected readonly pageWidthPoints = computed(() => {
    this.formValue();
    const size = PAGE_SIZE_POINTS[this.form.controls.pageSize.value ?? 'A4'];
    return this.form.controls.landscape.value ? size.height : size.width;
  });

  ngOnInit(): void {
    if (this.isNew) {
      this.fillForm({ id: '', name: '', pageSize: 'A4', landscape: true, backgroundFileId: null, elements: standardElements() });
    } else {
      this.load();
    }
  }

  hasUnsavedChanges(): boolean {
    return this.form.dirty;
  }

  protected load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.get(this.templateId!).subscribe({
      next: (template) => {
        this.fillForm(template);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.loadError.set(describeHttpError(error));
      },
    });
  }

  protected save(): void {
    if (this.saving()) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.formError.set(null);

    const request = this.buildRequest();
    const call = this.isNew ? this.api.create(request) : this.api.update(this.templateId!, request);
    call.subscribe({
      next: () => {
        this.toast.success('บันทึกแม่แบบแล้ว');
        this.form.markAsPristine();
        this.router.navigateByUrl('/admin/certificate-templates');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  protected openPdfPreview(): void {
    if (this.previewing()) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.previewing.set(true);
    this.api.preview(this.buildRequest()).subscribe({
      next: (blob) => {
        this.previewing.set(false);
        const address = URL.createObjectURL(blob);
        const opened = window.open(address, '_blank');
        if (!opened) {
          saveBlobAsFile(blob, 'certificate-preview.pdf');
        }
      },
      error: (error: unknown) => {
        this.previewing.set(false);
        this.toast.error(describeHttpError(error));
      },
    });
  }

  protected addElement(type: 'text' | 'image'): void {
    const element: CertificateElement =
      type === 'text'
        ? { type, x: 10, y: 10, width: 80, height: 8, text: 'ข้อความใหม่', fontSize: 24, bold: false, italic: false, underline: false, color: '#000000', align: 'center', imageFileId: null }
        : { type, x: 35, y: 75, width: 30, height: 12, text: null, fontSize: 24, bold: false, italic: false, underline: false, color: '#000000', align: 'center', imageFileId: null };
    this.elements.push(this.createElementGroup(element));
    this.selectedIndex.set(this.elements.length - 1);
    this.form.markAsDirty();
  }

  protected duplicateElement(index: number): void {
    const copy = this.toElement(this.elements.at(index));
    copy.y = Math.min(copy.y + 3, 100);
    this.elements.insert(index + 1, this.createElementGroup(copy));
    this.selectedIndex.set(index + 1);
    this.form.markAsDirty();
  }

  protected removeElement(index: number): void {
    this.elements.removeAt(index);
    this.selectedIndex.set(null);
    this.form.markAsDirty();
  }

  protected moveElement(index: number, step: -1 | 1): void {
    const target = index + step;
    if (target < 0 || target >= this.elements.length) {
      return;
    }
    const element = this.elements.at(index);
    this.elements.removeAt(index);
    this.elements.insert(target, element);
    this.selectedIndex.set(target);
    this.form.markAsDirty();
  }

  protected select(index: number): void {
    this.selectedIndex.set(index);
  }

  protected sampleText(text: string | null): string {
    let result = text ?? '';
    for (const variable of TEMPLATE_VARIABLES) {
      result = result.split(variable.token).join(variable.sample);
    }
    return result.trim();
  }

  protected fontSizeInContainerWidth(fontSize: number | null): number {
    return ((fontSize ?? 24) / this.pageWidthPoints()) * 100;
  }

  protected elementLabel(group: ElementGroup, index: number): string {
    return group.controls.type.value === 'text' ? `ข้อความ ${index + 1}` : `รูปภาพ ${index + 1}`;
  }

  private fillForm(template: CertificateTemplate): void {
    this.elements.clear();
    for (const element of template.elements) {
      this.elements.push(this.createElementGroup(element));
    }
    this.selectedIndex.set(null);
    this.form.reset({
      name: template.name,
      pageSize: template.pageSize,
      landscape: template.landscape,
      backgroundFileId: template.backgroundFileId,
      elements: this.elements.getRawValue(),
    });
  }

  private createElementGroup(element: CertificateElement): ElementGroup {
    return new FormGroup({
      type: new FormControl(element.type, { nonNullable: true }),
      x: new FormControl<number | null>(element.x, [Validators.required, Validators.min(0), Validators.max(100)]),
      y: new FormControl<number | null>(element.y, [Validators.required, Validators.min(0), Validators.max(100)]),
      width: new FormControl<number | null>(element.width, [Validators.required, Validators.min(1), Validators.max(100)]),
      height: new FormControl<number | null>(element.height, [Validators.required, Validators.min(1), Validators.max(100)]),
      text: new FormControl(element.text ?? '', { nonNullable: true, validators: Validators.maxLength(1000) }),
      fontSize: new FormControl<number | null>(element.fontSize, [Validators.required, Validators.min(6), Validators.max(200)]),
      bold: new FormControl(element.bold, { nonNullable: true }),
      italic: new FormControl(element.italic, { nonNullable: true }),
      underline: new FormControl(element.underline, { nonNullable: true }),
      color: new FormControl(element.color, { nonNullable: true, validators: [Validators.required, Validators.pattern(COLOR_PATTERN)] }),
      align: new FormControl(element.align, { nonNullable: true }),
      imageFileId: new FormControl<string | null>(element.imageFileId),
    });
  }

  private toElement(group: ElementGroup): CertificateElement {
    const value = group.getRawValue();
    const isText = value.type === 'text';
    return {
      type: value.type,
      x: value.x ?? 0,
      y: value.y ?? 0,
      width: value.width ?? 10,
      height: value.height ?? 10,
      text: isText ? value.text : null,
      fontSize: value.fontSize ?? 24,
      bold: value.bold,
      italic: value.italic,
      underline: value.underline,
      color: value.color,
      align: value.align,
      imageFileId: isText ? null : value.imageFileId,
    };
  }

  private buildRequest(): SaveCertificateTemplateRequest {
    const value = this.form.getRawValue();
    return {
      name: (value.name ?? '').trim(),
      pageSize: value.pageSize ?? 'A4',
      landscape: value.landscape ?? true,
      backgroundFileId: value.backgroundFileId,
      elements: this.elements.controls.map((group) => this.toElement(group)),
    };
  }
}
