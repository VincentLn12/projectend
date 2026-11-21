import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, Observable, startWith } from 'rxjs';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { urlValidator } from '../../../../shared/forms/validators';
import { BackLink } from '../../../../shared/navigation/back-link';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { AdminProjectsApi } from '../api/projects.api';
import {
  ADVISOR_ROLE_CO,
  ADVISOR_ROLE_MAIN,
  AdminProject,
  PickerOption,
  SaveProjectRequest,
} from '../types/project.types';

const BUDDHIST_ERA_OFFSET = 543;

@Component({
  selector: 'app-project-edit',
  imports: [ReactiveFormsModule, RouterLink, BackLink, Button, DataState, FileUpload, SelectInput, TextArea, TextInput, Toggle],
  templateUrl: './project-edit.html',
})
export class ProjectEdit implements OnInit {
  private readonly api = inject(AdminProjectsApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly projectId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.projectId === null;
  private readonly presetStudentId = this.route.snapshot.queryParamMap.get('studentId');
  protected readonly defaultAcademicYear = new Date().getFullYear() + BUDDHIST_ERA_OFFSET;

  protected readonly form = inject(FormBuilder).group({
    nameTH: ['', [Validators.required, Validators.maxLength(300)]],
    nameEN: ['', Validators.maxLength(300)],
    description: [''],
    academicYear: [this.defaultAcademicYear as number | null, [Validators.required, Validators.min(1900), Validators.max(2700)]],
    keyWords: ['', Validators.maxLength(500)],
    videoUrl: ['', [urlValidator, Validators.maxLength(500)]],
    webUrl: ['', [urlValidator, Validators.maxLength(500)]],
    githubUrl: ['', [urlValidator, Validators.maxLength(500)]],
    imageFileId: [null as string | null],
    pdfFileId: [null as string | null],
    studentId: [null as string | null, Validators.required],
    mainAdvisorId: [null as string | null],
    coAdvisorId: [null as string | null],
    isPublished: [false],
  });

  protected readonly students = signal<PickerOption[]>([]);
  protected readonly lecturers = signal<PickerOption[]>([]);
  protected readonly loadedProject = signal<AdminProject | null>(null);

  protected readonly studentOptions = computed<SelectOption<string>[]>(() =>
    this.students().map((student) => ({ value: student.id, label: student.label })),
  );
  protected readonly lecturerOptions = computed<SelectOption<string>[]>(() =>
    this.lecturers().map((lecturer) => ({ value: lecturer.id, label: lecturer.label })),
  );

  protected readonly formValue = toSignal(
    this.form.valueChanges.pipe(startWith(this.form.getRawValue())),
    { initialValue: this.form.getRawValue() },
  );

  protected readonly keywordTags = computed<string[]>(() => {
    const raw = (this.formValue().keyWords ?? '').trim();
    if (!raw) {
      return [];
    }
    return raw
      .split(',')
      .map((word) => word.trim())
      .filter((word) => word.length > 0);
  });

  protected readonly descriptionLength = computed(() => (this.formValue().description ?? '').trim().length);

  protected readonly activeLinksCount = computed(() => {
    const val = this.formValue();
    return [val.videoUrl, val.webUrl, val.githubUrl].filter((url) => (url ?? '').trim().length > 0).length;
  });

  protected readonly selectedStudentLabel = computed(() => {
    const id = this.formValue().studentId;
    if (!id) {
      return null;
    }
    return this.students().find((student) => student.id === id)?.label ?? this.loadedProject()?.studentName ?? null;
  });

  protected readonly selectedMainAdvisorLabel = computed(() => {
    const id = this.formValue().mainAdvisorId;
    if (!id) {
      return null;
    }
    return this.lecturers().find((lecturer) => lecturer.id === id)?.label ?? null;
  });

  protected readonly selectedCoAdvisorLabel = computed(() => {
    const id = this.formValue().coAdvisorId;
    if (!id) {
      return null;
    }
    return this.lecturers().find((lecturer) => lecturer.id === id)?.label ?? null;
  });

  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    const requests: Record<string, Observable<unknown>> = {
      students: this.api.students(),
      lecturers: this.api.lecturers(),
    };
    if (!this.isNew) {
      requests['project'] = this.api.get(this.projectId!);
    }

    forkJoin(requests).subscribe({
      next: (result) => {
        this.students.set(result['students'] as PickerOption[]);
        this.lecturers.set(result['lecturers'] as PickerOption[]);
        if (result['project']) {
          const project = result['project'] as AdminProject;
          this.loadedProject.set(project);
          this.fillForm(project);
        } else if (this.presetStudentId) {
          this.form.patchValue({ studentId: this.presetStudentId });
        }
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
    const existing = this.loadedProject();
    if (existing) {
      this.fillForm(existing);
    } else {
      this.form.reset({
        nameTH: '',
        nameEN: '',
        description: '',
        academicYear: this.defaultAcademicYear,
        keyWords: '',
        videoUrl: '',
        webUrl: '',
        githubUrl: '',
        imageFileId: null,
        pdfFileId: null,
        studentId: null,
        mainAdvisorId: null,
        coAdvisorId: null,
        isPublished: false,
      });
    }
  }

  protected save(): void {
    if (this.saving()) {
      return;
    }

    const { mainAdvisorId, coAdvisorId } = this.form.getRawValue();
    if (mainAdvisorId && mainAdvisorId === coAdvisorId) {
      this.form.controls.coAdvisorId.setErrors({ server: 'อาจารย์ที่ปรึกษาร่วมต้องเป็นคนละคนกับที่ปรึกษาหลัก' });
      this.form.controls.coAdvisorId.markAsTouched();
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.formError.set(null);

    const request = this.buildRequest();
    const call = this.isNew ? this.api.create(request) : this.api.update(this.projectId!, request);
    call.subscribe({
      next: () => {
        this.toast.success('บันทึกโครงงานแล้ว');
        this.router.navigateByUrl(this.presetStudentId ? `/admin/students/${this.presetStudentId}?tab=projects` : '/admin/projects');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(project: AdminProject): void {
    const mainAdvisor = project.advisors.find((advisor) => advisor.role === ADVISOR_ROLE_MAIN);
    const coAdvisor = project.advisors.find((advisor) => advisor.role === ADVISOR_ROLE_CO);

    this.form.reset({
      nameTH: project.nameTH,
      nameEN: project.nameEN ?? '',
      description: project.description ?? '',
      academicYear: project.academicYear,
      keyWords: project.keyWords ?? '',
      videoUrl: project.videoUrl ?? '',
      webUrl: project.webUrl ?? '',
      githubUrl: project.githubUrl ?? '',
      imageFileId: project.imageFileId,
      pdfFileId: project.pdfFileId,
      studentId: project.studentId,
      mainAdvisorId: mainAdvisor?.lecturerId ?? null,
      coAdvisorId: coAdvisor?.lecturerId ?? null,
      isPublished: project.isPublished,
    });
  }

  private buildRequest(): SaveProjectRequest {
    const value = this.form.getRawValue();
    const advisors: SaveProjectRequest['advisors'] = [];
    if (value.mainAdvisorId) {
      advisors.push({ lecturerId: value.mainAdvisorId, role: ADVISOR_ROLE_MAIN });
    }
    if (value.coAdvisorId) {
      advisors.push({ lecturerId: value.coAdvisorId, role: ADVISOR_ROLE_CO });
    }

    return {
      nameTH: (value.nameTH ?? '').trim(),
      nameEN: this.emptyToNull(value.nameEN),
      description: this.emptyToNull(value.description),
      academicYear: value.academicYear!,
      keyWords: this.emptyToNull(value.keyWords),
      videoUrl: this.emptyToNull(value.videoUrl),
      webUrl: this.emptyToNull(value.webUrl),
      githubUrl: this.emptyToNull(value.githubUrl),
      imageFileId: value.imageFileId,
      pdfFileId: value.pdfFileId,
      studentId: value.studentId!,
      isPublished: value.isPublished ?? false,
      advisors,
    };
  }

  private emptyToNull(text: string | null | undefined): string | null {
    const trimmed = (text ?? '').trim();
    return trimmed === '' ? null : trimmed;
  }
}
