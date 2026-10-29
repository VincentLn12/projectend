import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, Observable } from 'rxjs';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { urlValidator } from '../../../../shared/forms/validators';
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
  imports: [ReactiveFormsModule, RouterLink, Button, DataState, FileUpload, SelectInput, TextArea, TextInput, Toggle],
  templateUrl: './project-edit.html',
})
export class ProjectEdit implements OnInit {
  private readonly api = inject(AdminProjectsApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly projectId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.projectId === null;

  protected readonly form = inject(FormBuilder).group({
    nameTH: ['', [Validators.required, Validators.maxLength(300)]],
    nameEN: ['', Validators.maxLength(300)],
    description: [''],
    academicYear: [new Date().getFullYear() + BUDDHIST_ERA_OFFSET as number | null, [Validators.required, Validators.min(1900), Validators.max(2700)]],
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
  protected readonly studentOptions = computed<SelectOption<string>[]>(() =>
    this.students().map((student) => ({ value: student.id, label: student.label })),
  );
  protected readonly lecturerOptions = computed<SelectOption<string>[]>(() =>
    this.lecturers().map((lecturer) => ({ value: lecturer.id, label: lecturer.label })),
  );

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
          this.fillForm(result['project'] as AdminProject);
        }
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
        this.router.navigateByUrl('/admin/projects');
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
