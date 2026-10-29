import { Component, inject, OnInit, signal } from '@angular/core';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { LecturersApi } from '../api/lecturers.api';
import { Lecturer } from '../types/lecturer.types';

@Component({
  selector: 'app-public-lecturer-list',
  imports: [DataState, FileImage],
  templateUrl: './lecturer-list.html',
})
export class PublicLecturerList implements OnInit {
  private readonly api = inject(LecturersApi);

  protected readonly lecturers = signal<Lecturer[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.list().subscribe({
      next: (lecturers) => {
        this.lecturers.set(lecturers);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }
}
