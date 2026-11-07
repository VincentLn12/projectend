import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, Observable, startWith } from 'rxjs';
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
import { MyResearchProjectsApi } from '../api/research-projects.api';
import { ADVISOR_CO, ADVISOR_MAIN, LecturerOption, MyResearchProject, SaveMyResearchProjectRequest } from '../types/research-project.types';

const BUDDHIST_ERA_OFFSET = 543;
const YEARS_AHEAD = 1;
const YEARS_BACK = 8;

@Component({
  selector: 'app-my-research-project-edit',
  imports: [ReactiveFormsModule, RouterLink, Button, DataState, FileUpload, SelectInput, TextArea, TextInput, Toggle],
  templateUrl: './research-project-edit.html',
})
export class MyResearchProjectEdit implements OnInit {
  private readonly api = inject(MyResearchProjectsApi);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly projectId = inject(ActivatedRoute).snapshot.paramMap.get('id');

  protected readonly isNew = this.projectId === null;

  private readonly currentYear = new Date().getFullYear() + BUDDHIST_ERA_OFFSET;

  protected readonly form = inject(FormBuilder).group({
    nameTH: ['', [Validators.required, Validators.maxLength(300)]],
    nameEN: ['', Validators.maxLength(300)],
    academicYear: [this.currentYear as number | null, Validators.required],
    keyWords: ['', Validators.maxLength(500)],
    mainAdvisorId: [null as string | null],
    coAdvisorId: [null as string | null],
    description: [''],
    imageFileId: [null as string | null],
    pdfFileId: [null as string | null],
    videoUrl: ['', [Validators.maxLength(500), urlValidator]],
    webUrl: ['', [Validators.maxLength(500), urlValidator]],
    githubUrl: ['', [Validators.maxLength(500), urlValidator]],
    isPublished: [true],
  });

  protected readonly yearOptions: SelectOption<number>[] = Array.from({ length: YEARS_AHEAD + YEARS_BACK + 1 }, (_, index) => {
    const year = this.currentYear + YEARS_AHEAD - index;
    return { value: year, label: `${year}` };
  });

  protected readonly lecturers = signal<LecturerOption[]>([]);
  protected readonly lecturerOptions = computed<SelectOption<string>[]>(() =>
    this.lecturers().map((lecturer) => ({ value: lecturer.id, label: lecturer.label })),
  );

  private readonly formValue = toSignal(this.form.valueChanges.pipe(startWith(this.form.getRawValue())), {
    initialValue: this.form.getRawValue(),
  });

  protected readonly checklist = computed(() => {
    const value = this.formValue();
    const hasLink = !!(value.videoUrl?.trim() || value.webUrl?.trim() || value.githubUrl?.trim());
    return [
      { label: 'ชื่อโครงงาน', done: !!value.nameTH?.trim(), required: true },
      { label: 'อาจารย์ที่ปรึกษา', done: !!value.mainAdvisorId, required: false },
      { label: 'รายละเอียด', done: !!value.description?.trim(), required: false },
      { label: 'รูปปก', done: !!value.imageFileId, required: false },
      { label: 'ไฟล์ PDF เล่มโครงงาน', done: !!value.pdfFileId, required: false },
      { label: 'ลิงก์ วิดีโอ / เว็บ / GitHub', done: hasLink, required: false },
    ];
  });

  protected readonly doneCount = computed(() => this.checklist().filter((item) => item.done).length);

  protected readonly tags = computed(() =>
    (this.formValue().keyWords ?? '')
      .split(',')
      .map((word) => word.trim())
      .filter((word) => word.length > 0),
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

    const requests: Record<string, Observable<unknown>> = { lecturers: this.api.lecturers() };
    if (!this.isNew) {
      requests['project'] = this.api.get(this.projectId!);
    }
    forkJoin(requests).subscribe({
      next: (result) => {
        this.lecturers.set(result['lecturers'] as LecturerOption[]);
        if (result['project']) {
          this.fillForm(result['project'] as MyResearchProject);
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

    const value = this.form.getRawValue();
    if (value.mainAdvisorId && value.mainAdvisorId === value.coAdvisorId) {
      this.form.controls.coAdvisorId.setErrors({ server: 'เลือกอาจารย์คนเดียวกันซ้ำไม่ได้' });
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
        this.router.navigateByUrl('/my/projects');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(project: MyResearchProject): void {
    this.form.reset({
      nameTH: project.nameTH,
      nameEN: project.nameEN ?? '',
      academicYear: project.academicYear,
      keyWords: project.keyWords ?? '',
      mainAdvisorId: project.advisors.find((advisor) => advisor.role === ADVISOR_MAIN)?.lecturerId ?? null,
      coAdvisorId: project.advisors.find((advisor) => advisor.role === ADVISOR_CO)?.lecturerId ?? null,
      description: project.description ?? '',
      imageFileId: project.imageFileId,
      pdfFileId: project.pdfFileId,
      videoUrl: project.videoUrl ?? '',
      webUrl: project.webUrl ?? '',
      githubUrl: project.githubUrl ?? '',
      isPublished: project.isPublished,
    });
  }

  private buildRequest(): SaveMyResearchProjectRequest {
    const value = this.form.getRawValue();
    const advisors: { lecturerId: string; role: number }[] = [];
    if (value.mainAdvisorId) {
      advisors.push({ lecturerId: value.mainAdvisorId, role: ADVISOR_MAIN });
    }
    if (value.coAdvisorId) {
      advisors.push({ lecturerId: value.coAdvisorId, role: ADVISOR_CO });
    }

    return {
      nameTH: (value.nameTH ?? '').trim(),
      nameEN: (value.nameEN ?? '').trim() || null,
      description: (value.description ?? '').trim() || null,
      academicYear: value.academicYear!,
      keyWords: (value.keyWords ?? '').trim() || null,
      videoUrl: (value.videoUrl ?? '').trim() || null,
      webUrl: (value.webUrl ?? '').trim() || null,
      githubUrl: (value.githubUrl ?? '').trim() || null,
      imageFileId: value.imageFileId,
      pdfFileId: value.pdfFileId,
      isPublished: value.isPublished ?? false,
      advisors,
    };
  }
}
