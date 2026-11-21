import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { startWith } from 'rxjs';
import { HasUnsavedChanges } from '../../../../shared/forms/unsaved-changes.guard';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { BackLink } from '../../../../shared/navigation/back-link';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { SelectInput } from '../../../../shared/ui/select-input/select-input';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { AdminSyllabusesApi } from '../api/syllabuses.api';
import { AdminSyllabus, SUBJECT_CATEGORY_OPTIONS, SaveSyllabusRequest } from '../types/syllabus.types';

const BUDDHIST_ERA_OFFSET = 543;

@Component({
  selector: 'app-syllabus-edit',
  imports: [ReactiveFormsModule, RouterLink, BackLink, Button, DataState, FileUpload, SelectInput, TextArea, TextInput, Toggle],
  templateUrl: './syllabus-edit.html',
})
export class SyllabusEdit implements OnInit, HasUnsavedChanges {
  private readonly api = inject(AdminSyllabusesApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly syllabusId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.syllabusId === null;
  protected readonly defaultYear = new Date().getFullYear() + BUDDHIST_ERA_OFFSET;
  protected readonly categoryOptions = SUBJECT_CATEGORY_OPTIONS;

  protected readonly objectives = new FormArray<FormControl<string | null>>([]);
  protected readonly occupations = new FormArray<FormControl<string | null>>([]);
  protected readonly subjects = new FormArray<FormGroup>([]);

  protected readonly form = this.formBuilder.group({
    nameTH: ['', [Validators.required, Validators.maxLength(300)]],
    nameEN: ['', Validators.maxLength(300)],
    year: [this.defaultYear as number | null, [Validators.required, Validators.min(1900), Validators.max(2700)]],
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

  protected readonly loadedSyllabus = signal<AdminSyllabus | null>(null);
  protected readonly activeSubjectCategory = signal<number | 'all'>('all');

  protected readonly formValue = toSignal(
    this.form.valueChanges.pipe(startWith(this.form.getRawValue())),
    { initialValue: this.form.getRawValue() },
  );

  protected readonly subjectCreditsSum = computed(() => {
    const list = this.formValue().subjects ?? [];
    return list.reduce((sum, item) => sum + (Number(item['credit']) || 0), 0);
  });

  protected readonly creditsDifference = computed(() => {
    const value = this.formValue();
    const total = value.totalCredits;
    if (total === null || total === undefined || (value.subjects ?? []).length === 0) {
      return null;
    }
    const difference = total - this.subjectCreditsSum();
    return difference === 0 ? null : difference;
  });

  protected readonly categorySummary = computed(() => {
    const list = this.formValue().subjects ?? [];
    return this.categoryOptions.map((cat) => {
      const items = list.filter((s) => Number(s['subjectCategory']) === cat.value);
      const credits = items.reduce((sum, item) => sum + (Number(item['credit']) || 0), 0);
      return {
        value: cat.value,
        label: cat.label,
        count: items.length,
        credits,
      };
    });
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
        this.loadedSyllabus.set(syllabus);
        this.fillForm(syllabus);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.loadError.set(describeHttpError(error));
      },
    });
  }

  protected resetForm(): void {
    this.formError.set(null);
    const existing = this.loadedSyllabus();
    if (existing) {
      this.fillForm(existing);
    } else {
      this.objectives.clear();
      this.occupations.clear();
      this.subjects.clear();
      this.form.reset({
        nameTH: '',
        nameEN: '',
        year: this.defaultYear,
        degreeTH: '',
        degreeEN: '',
        totalCredits: null,
        pdfFileId: null,
        isCurrent: false,
        isPublished: false,
        objectives: [],
        occupations: [],
        subjects: [],
      });
    }
  }

  protected setSubjectCategoryFilter(category: number | 'all'): void {
    this.activeSubjectCategory.set(category);
  }

  protected matchesSubjectFilter(group: FormGroup): boolean {
    const filter = this.activeSubjectCategory();
    if (filter === 'all') {
      return true;
    }
    return Number(group.controls['subjectCategory']?.value) === filter;
  }

  protected addObjective(text = ''): void {
    this.objectives.push(new FormControl<string | null>(text, [Validators.required, Validators.maxLength(1000)]));
    this.form.markAsDirty();
  }

  protected addOccupation(text = ''): void {
    this.occupations.push(new FormControl<string | null>(text, [Validators.required, Validators.maxLength(300)]));
    this.form.markAsDirty();
  }

  protected addSubject(code = '', name = '', credit = 3, category?: number): void {
    const defaultCategory =
      category !== undefined
        ? category
        : this.activeSubjectCategory() === 'all'
          ? 0
          : (this.activeSubjectCategory() as number);

    this.subjects.push(
      this.formBuilder.group({
        code: [code, Validators.maxLength(20)],
        name: [name, [Validators.required, Validators.maxLength(200)]],
        credit: [credit as number | null, [Validators.required, Validators.min(0), Validators.max(999)]],
        subjectCategory: [defaultCategory as number | null, Validators.required],
      }),
    );
    this.form.markAsDirty();
  }

  protected moveSubject(index: number, step: -1 | 1): void {
    const target = index + step;
    if (target < 0 || target >= this.subjects.length) {
      return;
    }
    const group = this.subjects.at(index);
    this.subjects.removeAt(index);
    this.subjects.insert(target, group);
    this.form.markAsDirty();
  }

  hasUnsavedChanges(): boolean {
    return this.form.dirty;
  }

  protected removeAt(list: FormArray, index: number): void {
    list.removeAt(index);
    this.form.markAsDirty();
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
        this.form.markAsPristine();
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
    this.form.markAsPristine();
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
