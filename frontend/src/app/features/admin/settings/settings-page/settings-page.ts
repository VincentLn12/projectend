import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { startWith } from 'rxjs';
import { applyServerErrors, describeHttpError } from '../../../../shared/forms/server-errors';
import { urlValidator } from '../../../../shared/forms/validators';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileUpload } from '../../../../shared/ui/file-upload/file-upload';
import { TextArea } from '../../../../shared/ui/text-area/text-area';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminSettingsApi } from '../api/settings.api';
import { QuickMenuItemSetting, SiteSettings } from '../types/settings.types';

const QUICK_MENU_LABELS: Record<string, string> = {
  syllabus: 'หลักสูตร',
  'tech-subjects': 'เราเรียนอะไร',
  projects: 'โครงงานวิจัย',
  lecturers: 'อาจารย์',
  courses: 'หลักสูตรอบรม',
  news: 'ข่าวสาร',
};

type QuickMenuItemGroup = FormGroup<{
    key: FormControl<string>;
    enabled: FormControl<boolean>;
    title: FormControl<string>;
    imageFileId: FormControl<string | null>;
  }>;

@Component({
  selector: 'app-settings-page',
  imports: [ReactiveFormsModule, Button, DataState, FileUpload, TextArea, TextInput, Toggle],
  templateUrl: './settings-page.html',
})
export class SettingsPage implements OnInit {
  private readonly api = inject(AdminSettingsApi);
  private readonly toast = inject(ToastService);

  protected readonly form = inject(FormBuilder).group({
    webName: ['', [Validators.required, Validators.maxLength(200)]],
    universityName: ['', Validators.maxLength(200)],
    footerText: ['', Validators.maxLength(500)],
    registerUrl: ['', [urlValidator, Validators.maxLength(500)]],
    kruUrl: ['', [urlValidator, Validators.maxLength(500)]],
    pageFacebook: ['', [urlValidator, Validators.maxLength(500)]],
    logoFileId: [null as string | null],
    videoUrl: ['', Validators.maxLength(500)],
    mapLocation: ['', Validators.maxLength(500)],
    aboutTitle: ['', Validators.maxLength(300)],
    aboutDescription: ['', Validators.maxLength(2000)],
    quickMenuEnabled: [true],
    quickMenuBackgroundFileId: [null as string | null],
    quickMenuItems: new FormArray<QuickMenuItemGroup>([]),
  });

  protected get quickMenuItems(): FormArray<QuickMenuItemGroup> {
    return this.form.controls.quickMenuItems;
  }

  private readonly loadedSettings = signal<SiteSettings | null>(null);
  private readonly formValue = toSignal(this.form.valueChanges.pipe(startWith(this.form.getRawValue())), {
    initialValue: this.form.getRawValue(),
  });

  protected readonly videoCount = computed(
    () => (this.formValue().videoUrl ?? '').split(',').filter((url) => url.trim().length > 0).length,
  );

  protected readonly mapLinkUrl = computed(() => {
    const location = (this.formValue().mapLocation ?? '').trim();
    return location ? `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(location)}` : null;
  });

  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.get().subscribe({
      next: (settings) => {
        this.fillForm(settings);
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

    this.api.save(this.buildRequest()).subscribe({
      next: (saved) => {
        this.saving.set(false);
        this.fillForm(saved);
        this.toast.success('บันทึกการตั้งค่าแล้ว');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      },
    });
  }

  protected resetForm(): void {
    this.formError.set(null);
    const existing = this.loadedSettings();
    if (existing) {
      this.fillForm(existing);
    }
  }

  protected quickMenuLabel(key: string): string {
    return QUICK_MENU_LABELS[key] ?? key;
  }

  protected moveQuickMenuItem(index: number, step: -1 | 1): void {
    const target = index + step;
    if (target < 0 || target >= this.quickMenuItems.length) {
      return;
    }
    const item = this.quickMenuItems.at(index);
    this.quickMenuItems.removeAt(index);
    this.quickMenuItems.insert(target, item);
    this.form.markAsDirty();
  }

  private mergeQuickMenuItems(saved: QuickMenuItemSetting[]): QuickMenuItemSetting[] {
    const knownKeys = Object.keys(QUICK_MENU_LABELS);
    const ordered = (saved ?? []).filter((item) => knownKeys.includes(item.key));
    const missing = knownKeys
      .filter((key) => !ordered.some((item) => item.key === key))
      .map((key) => ({ key, enabled: true, title: null, imageFileId: null }));
    return [...ordered, ...missing];
  }

  private fillForm(settings: SiteSettings): void {
    this.quickMenuItems.clear();
    for (const item of this.mergeQuickMenuItems(settings.quickMenuItems)) {
      this.quickMenuItems.push(
        new FormGroup({
          key: new FormControl(item.key, { nonNullable: true }),
          enabled: new FormControl(item.enabled, { nonNullable: true }),
          title: new FormControl(item.title ?? '', { nonNullable: true, validators: Validators.maxLength(100) }),
          imageFileId: new FormControl(item.imageFileId ?? null),
        }),
      );
    }

    this.loadedSettings.set(settings);
    this.form.reset({
      webName: settings.webName,
      universityName: settings.universityName ?? '',
      footerText: settings.footerText ?? '',
      registerUrl: settings.registerUrl ?? '',
      kruUrl: settings.kruUrl ?? '',
      pageFacebook: settings.pageFacebook ?? '',
      logoFileId: settings.logoFileId,
      videoUrl: settings.videoUrl ?? '',
      mapLocation: settings.mapLocation ?? '',
      aboutTitle: settings.aboutTitle ?? '',
      aboutDescription: settings.aboutDescription ?? '',
      quickMenuEnabled: settings.quickMenuEnabled,
      quickMenuBackgroundFileId: settings.quickMenuBackgroundFileId,
      quickMenuItems: this.quickMenuItems.getRawValue(),
    });
  }

  private buildRequest(): SiteSettings {
    const value = this.form.getRawValue();
    return {
      webName: (value.webName ?? '').trim(),
      universityName: this.emptyToNull(value.universityName),
      footerText: this.emptyToNull(value.footerText),
      registerUrl: this.emptyToNull(value.registerUrl),
      kruUrl: this.emptyToNull(value.kruUrl),
      pageFacebook: this.emptyToNull(value.pageFacebook),
      logoFileId: value.logoFileId,
      videoUrl: this.emptyToNull(value.videoUrl),
      mapLocation: this.emptyToNull(value.mapLocation),
      aboutTitle: this.emptyToNull(value.aboutTitle),
      aboutDescription: this.emptyToNull(value.aboutDescription),
      quickMenuEnabled: value.quickMenuEnabled ?? true,
      quickMenuBackgroundFileId: value.quickMenuBackgroundFileId,
      quickMenuItems: (value.quickMenuItems ?? []).map((item) => ({
        key: item.key ?? '',
        enabled: item.enabled ?? true,
        title: this.emptyToNull(item.title),
        imageFileId: item.imageFileId ?? null,
      })),
    };
  }

  private emptyToNull(text: string | null | undefined): string | null {
    const trimmed = (text ?? '').trim();
    return trimmed === '' ? null : trimmed;
  }
}
