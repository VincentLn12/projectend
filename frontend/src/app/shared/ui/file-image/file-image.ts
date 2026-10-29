import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { FilesApi } from '../../../core/files/api/files.api';

// Shows an uploaded picture by its id. When there is no id, or the file cannot be loaded
// (for example a migrated file that has not been copied yet), a grey placeholder is drawn in its place.
//   <app-file-image [fileId]="news.mainImageFileId" alt="..." class="h-40 w-full" />
@Component({
  selector: 'app-file-image',
  templateUrl: './file-image.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block overflow-hidden bg-slate-100' },
})
export class FileImage {
  private readonly filesApi = inject(FilesApi);

  readonly fileId = input<string | null | undefined>(null);
  readonly alt = input('');

  private readonly failedId = signal<string | null>(null);

  protected readonly source = computed(() => {
    const id = this.fileId();
    return id && this.failedId() !== id ? this.filesApi.url(id) : null;
  });

  protected markFailed(): void {
    this.failedId.set(this.fileId() ?? null);
  }
}
