import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FilesApi } from '../../../../core/files/api/files.api';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { PublicStudentProjectsApi } from '../api/student-projects.api';
import { SEMESTER_LABELS, StudentProjectDetail } from '../types/student-project.types';

const YOUTUBE_ID_PATTERN = /(?:youtube\.com\/(?:watch\?v=|embed\/|shorts\/)|youtu\.be\/)([\w-]{11})/;

@Component({
  selector: 'app-public-student-project-detail',
  imports: [RouterLink, Button, DataState, FileImage],
  templateUrl: './student-project-detail.html',
})
export class PublicStudentProjectDetail implements OnInit {
  private readonly api = inject(PublicStudentProjectsApi);
  private readonly filesApi = inject(FilesApi);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly projectId = inject(ActivatedRoute).snapshot.paramMap.get('id')!;

  protected readonly project = signal<StudentProjectDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly semesterLabels = SEMESTER_LABELS;

  protected readonly tags = computed(() =>
    (this.project()?.technologies ?? '')
      .split(',')
      .map((word) => word.trim())
      .filter((word) => word.length > 0),
  );

  protected readonly videoEmbedUrl = computed<SafeResourceUrl | null>(() => {
    const url = this.project()?.videoUrl?.trim();
    const youtubeId = url?.match(YOUTUBE_ID_PATTERN)?.[1];
    return youtubeId ? this.sanitizer.bypassSecurityTrustResourceUrl(`https://www.youtube-nocookie.com/embed/${youtubeId}`) : null;
  });

  protected readonly pdfUrl = computed(() => {
    const id = this.project()?.pdfFileId;
    return id ? this.filesApi.url(id) : null;
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.get(this.projectId).subscribe({
      next: (project) => {
        this.project.set(project);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }
}
