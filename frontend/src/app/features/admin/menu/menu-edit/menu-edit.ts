import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { TextInput } from '../../../../shared/ui/text-input/text-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { Toggle } from '../../../../shared/ui/toggle/toggle';
import { AdminMenuApi, describeMenuError } from '../api/menu.api';
import {
  AdminMenuItem,
  KIND_EXTERNAL,
  KIND_GROUP,
  KIND_LABELS,
  KIND_PAGE,
  MenuPageOption,
  SaveMenuItemRequest,
} from '../types/menu.types';

@Component({
  selector: 'app-menu-edit',
  imports: [ReactiveFormsModule, RouterLink, Button, DataState, SelectInput, TextInput, Toggle],
  templateUrl: './menu-edit.html',
})
export class MenuEdit implements OnInit {
  private readonly api = inject(AdminMenuApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly itemId = this.route.snapshot.paramMap.get('id');
  protected readonly isNew = this.itemId === null;

  protected readonly kindOptions: SelectOption<number>[] = Object.entries(KIND_LABELS).map(([value, label]) => ({
    value: Number(value),
    label,
  }));

  protected readonly form = inject(FormBuilder).group({
    label: ['', [Validators.required, Validators.maxLength(100)]],
    kind: [KIND_PAGE as number | null, Validators.required],
    page: [null as string | null],
    url: ['', Validators.maxLength(500)],
    parentId: [null as string | null],
    openInNewTab: [false],
    isVisible: [true],
  });

  protected readonly pages = signal<MenuPageOption[]>([]);
  protected readonly groups = signal<AdminMenuItem[]>([]);
  protected readonly selectedKind = signal<number | null>(KIND_PAGE);

  protected readonly isPage = computed(() => this.selectedKind() === KIND_PAGE);
  protected readonly isExternal = computed(() => this.selectedKind() === KIND_EXTERNAL);
  protected readonly isGroup = computed(() => this.selectedKind() === KIND_GROUP);

  protected readonly pageOptions = computed<SelectOption<string>[]>(() =>
    this.pages().map((page) => ({ value: page.path, label: page.label })),
  );
  protected readonly parentOptions = computed<SelectOption<string | null>[]>(() => [
    { value: null, label: 'เมนูหลัก (ไม่อยู่ในกลุ่ม)' },
    ...this.groups().map((group) => ({ value: group.id as string | null, label: `อยู่ในกลุ่ม: ${group.label}` })),
  ]);

  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly formError = signal<string | null>(null);

  constructor() {
    this.form.controls.kind.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((kind) => this.selectedKind.set(kind));
  }

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    forkJoin({
      pages: this.api.pages(),
      items: this.api.list(),
      item: this.isNew ? [null] : this.api.get(this.itemId!),
    }).subscribe({
      next: ({ pages, items, item }) => {
        this.pages.set(pages);
        this.groups.set(items.filter((other) => other.kind === KIND_GROUP && other.parentId === null && other.id !== this.itemId));
        if (item) {
          this.fillForm(item);
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
    this.checkTarget();
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
        this.toast.success('บันทึกเมนูแล้ว');
        this.router.navigateByUrl('/admin/menu');
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.formError.set(describeMenuError(error));
      },
    });
  }

  // The page or the address is required only for the kind of menu that uses it.
  private checkTarget(): void {
    const { page, url } = this.form.getRawValue();

    if (this.isPage() && !page) {
      this.form.controls.page.setErrors({ server: 'กรุณาเลือกหน้า' });
    }
    if (this.isExternal()) {
      const address = (url ?? '').trim();
      if (!/^https?:\/\/\S+$/i.test(address)) {
        this.form.controls.url.setErrors({ server: 'ต้องขึ้นต้นด้วย http:// หรือ https:// เช่น https://www.kru.ac.th' });
      }
    }
  }

  private fillForm(item: AdminMenuItem): void {
    this.form.reset({
      label: item.label,
      kind: item.kind,
      page: item.kind === KIND_PAGE ? item.target : null,
      url: item.kind === KIND_EXTERNAL ? (item.target ?? '') : '',
      parentId: item.parentId,
      openInNewTab: item.openInNewTab,
      isVisible: item.isVisible,
    });
    this.selectedKind.set(item.kind);
  }

  private buildRequest(): SaveMenuItemRequest {
    const value = this.form.getRawValue();
    const kind = value.kind ?? KIND_PAGE;

    return {
      label: (value.label ?? '').trim(),
      kind,
      target: kind === KIND_PAGE ? value.page : kind === KIND_EXTERNAL ? (value.url ?? '').trim() : null,
      parentId: kind === KIND_GROUP ? null : value.parentId,
      isVisible: value.isVisible ?? true,
      openInNewTab: kind === KIND_GROUP ? false : (value.openInNewTab ?? false),
    };
  }
}
