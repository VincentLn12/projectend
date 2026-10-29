import { Component, inject, OnInit, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { Checkbox } from '../../../../shared/ui/checkbox/checkbox';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { SelectInput } from '../../../../shared/ui/select-input/select-input';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { AdminSyllabusesApi } from '../api/syllabuses.api';
import { AdminSyllabus, SUBJECT_CATEGORY_OPTIONS, SaveSyllabusRequest } from '../types/syllabus.types';

@Component({
  selector: 'app-syllabus-edit',
  imports: [ReactiveFormsModule, RouterLink, Button, Checkbox, DataState, FileUpload, SelectInput, TextInput, Toggle],
  templateUrl: './syllabus-edit.html',
})
export class SyllabusEdit implements OnInit {
  private readonly api = inject(AdminSyllabusesApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly syllabusId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.syllabusId === null;
  protected readonly categoryOptions = SUBJECT_CATEGORY_OPTIONS;

  protected readonly objectives = new FormArray<FormControl<string | null>>([]);
  protected readonly occupations = new FormArray<FormControl<string | null>>([]);
  protected readonly subjects = new FormArray<FormGroup>([]);

  protected readonly form = this.formBuilder.group({
    nameTH: ['', [Validators.required, Validators.maxLength(300)]],
    nameEN: ['', Validators.maxLength(300)],
    year: [new Date().getFullYear() + 543 as number | null, [Validators.required, Validators.min(1900), Validators.max(2700)]],
    degreeTH: ['', Validators.maxLength(200)],
    degreeEN: ['', Validators.maxLength(200)],
    totalCredits: [null as number | null, [Validators.min(0), Validators.max(400)]],
    pdfFileId: [null as string | null],
    isCurrent: [false],
    isPublished: [false],
    objectives: this.objectives,
    occupations: this.occupations,
    subjects: this.subjects,
  });

  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  ngOnInit(): void {
    if (!this.isNew) {
      this.load();
    }
  }

  protected load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.get(this.syllabusId!).subscribe({
      next: (syllabus) => {
        this.fillForm(syllabus);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.loadError.set(describeHttpError(error));
      },
    });
  }

  protected addObjective(text = ''): void {
    this.objectives.push(new FormControl<string | null>(text, [Validators.required, Validators.maxLength(1000)]));
  }

  protected addOccupation(text = ''): void {
    this.occupations.push(new FormControl<string | null>(text, [Validators.required, Validators.maxLength(300)]));
  }

  protected addSubject(code = '', name = '', credit = 0, category = 0): void {
    this.subjects.push(
      this.formBuilder.group({
        code: [code, Validators.maxLength(20)],
        name: [name, [Validators.required, Validators.maxLength(200)]],
        credit: [credit as number | null, [Validators.required, Validators.min(0), Validators.max(999)]],
        subjectCategory: [category as number | null, Validators.required],
      }),
    );
  }

  protected removeAt(list: FormArray, index: number): void {
    list.removeAt(index);
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
    const call = this.isNew ? this.api.create(request) : this.api.update(this.syllabusId!, request);
    call.subscribe({
      next: () => {
        this.toast.success('บันทึกหลักสูตรแล้ว');
        this.router.navigateByUrl('/admin/syllabuses');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(syllabus: AdminSyllabus): void {
    this.objectives.clear();
    this.occupations.clear();
    this.subjects.clear();
    syllabus.objectives.forEach((text) => this.addObjective(text));
    syllabus.occupations.forEach((text) => this.addOccupation(text));
    syllabus.subjects.forEach((subject) => this.addSubject(subject.code ?? '', subject.name, subject.credit, subject.subjectCategory));

    this.form.patchValue({
      nameTH: syllabus.nameTH,
      nameEN: syllabus.nameEN ?? '',
      year: syllabus.year,
      degreeTH: syllabus.degreeTH ?? '',
      degreeEN: syllabus.degreeEN ?? '',
      totalCredits: syllabus.totalCredits,
      pdfFileId: syllabus.pdfFileId,
      isCurrent: syllabus.isCurrent,
      isPublished: syllabus.isPublished,
    });
  }

  private buildRequest(): SaveSyllabusRequest {
    const value = this.form.getRawValue();
    return {
      nameTH: (value.nameTH ?? '').trim(),
      nameEN: this.emptyToNull(value.nameEN),
      year: value.year!,
      degreeTH: this.emptyToNull(value.degreeTH),
      degreeEN: this.emptyToNull(value.degreeEN),
      totalCredits: value.totalCredits,
      pdfFileId: value.pdfFileId,
      isCurrent: value.isCurrent ?? false,
      isPublished: value.isPublished ?? false,
      objectives: this.objectives.getRawValue().map((text) => (text ?? '').trim()),
      occupations: this.occupations.getRawValue().map((text) => (text ?? '').trim()),
      subjects: this.subjects.getRawValue().map((subject) => ({
        code: this.emptyToNull(subject['code']),
        name: String(subject['name'] ?? '').trim(),
        credit: Number(subject['credit'] ?? 0),
        subjectCategory: Number(subject['subjectCategory'] ?? 0),
      })),
    };
  }

  private emptyToNull(text: string | null | undefined): string | null {
    const trimmed = (text ?? '').trim();
    return trimmed === '' ? null : trimmed;
  }
}
