import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { startWith } from 'rxjs';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { ThaiDatePipe } from '../../../../shared/pipes/thai-date.pipe';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { AdminNewsApi } from '../api/news.api';
import { AdminNews, SaveNewsRequest } from '../types/news.types';

@Component({
  selector: 'app-news-edit',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    ThaiDatePipe,
    Button,
    DataState,
    FileUpload,
    TextArea,
    TextInput,
    Toggle,
  ],
  templateUrl: './news-edit.html',
})
export class NewsEdit implements OnInit {
  private readonly api = inject(AdminNewsApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  protected readonly newsId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.newsId === null;

  protected readonly form = inject(FormBuilder).nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(300)]],
    subTitle: ['', Validators.maxLength(500)],
    body: ['', Validators.required],
    isPublished: [false],
    fileIds: [[] as string[]],
  });

  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly loadedNews = signal<AdminNews | null>(null);

  protected readonly formValue = toSignal(
    this.form.valueChanges.pipe(startWith(this.form.getRawValue())),
    { initialValue: this.form.getRawValue() },
  );

  protected readonly galleryFileIds = computed(() => this.formValue().fileIds ?? []);

  protected readonly bodyStats = computed(() => {
    const rawBody = (this.formValue().body ?? '').trim();
    if (!rawBody) {
      return { characters: 0, paragraphs: 0, readingMinutes: 0 };
    }
    const characters = rawBody.length;
    const paragraphs = rawBody.split(/\r?\n+/).filter((line) => line.trim().length > 0).length;
    const readingMinutes = Math.max(1, Math.ceil(characters / 450));
    return { characters, paragraphs, readingMinutes };
  });

  ngOnInit(): void {
    if (!this.isNew) {
      this.loadNews();
    }
  }

  protected loadNews(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.get(this.newsId!).subscribe({
      next: (news) => {
        this.loadedNews.set(news);
        this.fillForm(news);
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
    const existing = this.loadedNews();
    if (existing) {
      this.fillForm(existing);
    } else {
      this.form.reset({
        title: '',
        subTitle: '',
        body: '',
        isPublished: false,
        fileIds: [],
      });
    }
  }

  protected async askToDelete(): Promise<void> {
    if (this.isNew || !this.newsId) {
      return;
    }
    const currentTitle = this.form.controls.title.value.trim() || this.loadedNews()?.title || 'ข่าวนี้';
    const confirmed = await this.confirm.ask({
      title: 'ยืนยันการลบข่าว',
      message: `ต้องการลบข่าว "${currentTitle}" ใช่หรือไม่? การดำเนินการนี้ไม่สามารถย้อนกลับได้`,
      confirmText: 'ลบข่าว',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.saving.set(true);
    this.api.delete(this.newsId).subscribe({
      next: () => {
        this.toast.success('ลบข่าวแล้ว');
        this.router.navigateByUrl('/admin/news');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.toast.error(describeHttpError(error));
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
    const call = this.isNew ? this.api.create(request) : this.api.update(this.newsId!, request);
    call.subscribe({
      next: () => {
        this.toast.success('บันทึกข่าวแล้ว');
        this.router.navigateByUrl('/admin/news');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  private fillForm(news: AdminNews): void {
    // The first id is the main picture, as in the upload component.
    const orderedFiles = [...news.files].sort(
      (a, b) => Number(b.isMain) - Number(a.isMain) || a.sortOrder - b.sortOrder,
    );
    this.form.reset({
      title: news.title,
      subTitle: news.subTitle ?? '',
      body: news.body,
      isPublished: news.isPublished,
      fileIds: orderedFiles.map((file) => file.fileId),
    });
  }

  private buildRequest(): SaveNewsRequest {
    const value = this.form.getRawValue();
    return {
      title: value.title.trim(),
      subTitle: value.subTitle.trim() || null,
      body: value.body,
      isPublished: value.isPublished,
      fileIds: value.fileIds,
      mainFileId: value.fileIds[0] ?? null,
    };
  }
}
