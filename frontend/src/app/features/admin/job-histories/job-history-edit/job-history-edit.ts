import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, Observable } from 'rxjs';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { DateInput } from '../../../../shared/ui/date-input/date-input';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminJobHistoriesApi } from '../api/job-histories.api';
import { AdminJobHistory, PickerOption, SaveJobHistoryRequest } from '../types/job-history.types';

@Component({
  selector: 'app-job-history-edit',
  imports: [ReactiveFormsModule, RouterLink, Button, DataState, SelectInput, TextArea, TextInput, DateInput, Toggle],
  templateUrl: './job-history-edit.html',
})
export class JobHistoryEdit implements OnInit {
  private readonly api = inject(AdminJobHistoriesApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly itemId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.itemId === null;

  protected readonly form = inject(FormBuilder).group({
    userId: [null as string | null, Validators.required],
    jobName: ['' as string | null, [Validators.required, Validators.maxLength(200)]],
    company: ['' as string | null, Validators.maxLength(200)],
    position: ['' as string | null, Validators.maxLength(200)],
    startDate: ['' as string | null, Validators.required],
    description: ['' as string | null, Validators.maxLength(1000)],
    isPublished: [false],
  });

  protected readonly userIdChoices = signal<PickerOption[]>([]);
  protected readonly userIdOptions = computed<SelectOption<string>[]>(() =>
    this.userIdChoices().map((choice) => ({ value: choice.id, label: choice.label })),
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

    const requests: Record<string, Observable<unknown>> = { choices: this.api.students() };
    if (!this.isNew) {
      requests['item'] = this.api.get(this.itemId!);
    }
    forkJoin(requests).subscribe({
      next: (result) => {
        this.userIdChoices.set(result['choices'] as PickerOption[]);
        if (result['item']) {
          this.fillForm(result['item'] as AdminJobHistory);
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
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.formError.set(null);

    const request = this.buildRequest();
    const call = this.isNew ? this.api.create(request) : this.api.update(this.itemId!, request);
    call.subscribe({
      next: () => {
        this.toast.success('บันทึกประวัติการทำงานแล้ว');
        this.router.navigateByUrl('/admin/job-histories');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(entity: AdminJobHistory): void {
    this.form.reset({
      userId: entity.userId,
      jobName: entity.jobName ?? '',
      company: entity.company ?? '',
      position: entity.position ?? '',
      startDate: entity.startDate,
      description: entity.description ?? '',
      isPublished: entity.isPublished,
    });
  }

  private buildRequest(): SaveJobHistoryRequest {
    const value = this.form.getRawValue();
    return {
      userId: value.userId!,
      jobName: (value.jobName ?? '').trim(),
      company: this.emptyToNull(value.company),
      position: this.emptyToNull(value.position),
      startDate: value.startDate ?? '',
      description: this.emptyToNull(value.description),
      isPublished: value.isPublished ?? false,
    };
  }

  private emptyToNull(text: string | null | undefined): string | null {
    const trimmed = (text ?? '').trim();
    return trimmed === '' ? null : trimmed;
  }
}
