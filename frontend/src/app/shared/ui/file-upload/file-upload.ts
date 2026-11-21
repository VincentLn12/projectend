import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { FilesApi } from '../../../core/files/api/files.api';
import { FileDto } from '../../../core/files/types/file.types';
import { FormControlBase } from '../../forms/form-control-base';
import { Field } from '../field/field';

export type AcceptedFiles = 'image' | 'pdf' | 'any';

const IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
const PDF_TYPE = 'application/pdf';
const MAX_BYTES = 10 * 1024 * 1024; // the API refuses anything larger

// Uploads pictures or PDFs to the API and keeps their ids.
// Single mode: the value is one id (or null). Multiple mode: the value is a list of ids, and the first one
// is the main picture (the API treats the first id as the main one unless told otherwise).
@Component({
  selector: 'app-file-upload',
  imports: [Field],
  templateUrl: './file-upload.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FileUpload extends FormControlBase<string | string[]> {
  private readonly filesApi = inject(FilesApi);

  readonly multiple = input(false);
  readonly accept = input<AcceptedFiles>('image');

  protected readonly uploadingCount = signal(0);
  protected readonly uploadError = signal<string | null>(null);
  protected readonly isDragging = signal(false);

  // What we learned about files uploaded in this session. Files that were already saved earlier are known only by id.
  private readonly uploaded = signal<Record<string, FileDto>>({});
  private readonly brokenPreviews = signal<ReadonlySet<string>>(new Set());

  protected readonly ids = computed<string[]>(() => {
    const current = this.value();
    if (Array.isArray(current)) {
      return current;
    }
    return current ? [current] : [];
  });

  protected readonly acceptAttribute = computed(() => {
    const types = this.accept() === 'pdf' ? [PDF_TYPE] : this.accept() === 'image' ? IMAGE_TYPES : [...IMAGE_TYPES, PDF_TYPE];
    return types.join(',');
  });

  protected readonly typeHint = computed(() => {
    const kinds = this.accept() === 'pdf' ? 'PDF' : this.accept() === 'image' ? 'JPG, PNG, GIF, WebP' : 'JPG, PNG, GIF, WebP, PDF';
    return `${kinds} ขนาดไม่เกิน 10 MB`;
  });

  protected url(id: string): string {
    return this.filesApi.url(id);
  }

  protected showsAsImage(id: string): boolean {
    const known = this.uploaded()[id];
    if (known) {
      return known.contentType.startsWith('image/');
    }
    return this.accept() !== 'pdf' && !this.brokenPreviews().has(id);
  }

  protected fileName(id: string): string {
    return this.uploaded()[id]?.originalName ?? 'เปิดไฟล์';
  }

  protected previewFailed(id: string): void {
    this.brokenPreviews.update((set) => new Set(set).add(id));
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    if (!this.isDisabled() && this.uploadingCount() === 0) {
      this.isDragging.set(true);
    }
  }

  protected onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  protected async onDrop(event: DragEvent): Promise<void> {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
    if (this.isDisabled() || this.uploadingCount() > 0) {
      return;
    }
    const files = Array.from(event.dataTransfer?.files ?? []);
    await this.processFiles(files);
  }

  protected async onFilesChosen(event: Event): Promise<void> {
    const picker = event.target as HTMLInputElement;
    const files = Array.from(picker.files ?? []);
    picker.value = ''; // so choosing the same file again still fires an event
    await this.processFiles(files);
  }

  protected remove(id: string): void {
    this.emit(this.ids().filter((existing) => existing !== id));
    this.markTouched();
  }

  protected clearAll(): void {
    this.emit([]);
    this.markTouched();
  }

  protected makeMain(id: string): void {
    this.emit([id, ...this.ids().filter((existing) => existing !== id)]);
  }

  protected moveLeft(index: number): void {
    const current = [...this.ids()];
    if (index <= 0 || index >= current.length) {
      return;
    }
    [current[index - 1], current[index]] = [current[index], current[index - 1]];
    this.emit(current);
  }

  protected moveRight(index: number): void {
    const current = [...this.ids()];
    if (index < 0 || index >= current.length - 1) {
      return;
    }
    [current[index], current[index + 1]] = [current[index + 1], current[index]];
    this.emit(current);
  }

  private async processFiles(files: File[]): Promise<void> {
    this.markTouched();

    if (files.length === 0) {
      return;
    }

    const chosen = this.multiple() ? files : files.slice(0, 1);
    this.uploadError.set(null);

    for (const file of chosen) {
      const problem = this.checkFile(file);
      if (problem) {
        this.uploadError.set(problem);
        return;
      }
    }

    this.uploadingCount.set(chosen.length);
    const results = await Promise.allSettled(chosen.map((file) => firstValueFrom(this.filesApi.upload(file))));
    this.uploadingCount.set(0);

    const saved: FileDto[] = [];
    let failure: unknown = null;
    for (const result of results) {
      if (result.status === 'fulfilled') {
        saved.push(result.value);
      } else {
        failure = failure ?? result.reason;
      }
    }

    if (failure) {
      this.uploadError.set(this.messageFor(failure));
    }
    if (saved.length === 0) {
      return;
    }

    this.uploaded.update((known) => {
      const next = { ...known };
      for (const file of saved) {
        next[file.id] = file;
      }
      return next;
    });
    this.emit(this.multiple() ? [...this.ids(), ...saved.map((file) => file.id)] : [saved[0].id]);
  }

  private emit(ids: string[]): void {
    this.update(this.multiple() ? ids : (ids[0] ?? null));
  }

  private checkFile(file: File): string | null {
    const allowed = this.acceptAttribute().split(',');
    if (!allowed.includes(file.type)) {
      return `ไฟล์ "${file.name}" ไม่ใช่ชนิดที่รองรับ (${this.typeHint()})`;
    }
    if (file.size > MAX_BYTES) {
      return `ไฟล์ "${file.name}" ใหญ่เกิน 10 MB`;
    }
    return null;
  }

  private messageFor(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 400 && typeof error.error?.detail === 'string') {
        return error.error.detail;
      }
      if (error.status === 401 || error.status === 403) {
        return 'ไม่มีสิทธิ์อัปโหลดไฟล์ กรุณาเข้าสู่ระบบอีกครั้ง';
      }
      if (error.status === 413) {
        return 'ไฟล์ใหญ่เกินกำหนด';
      }
    }
    return 'อัปโหลดไม่สำเร็จ กรุณาลองใหม่';
  }
}
