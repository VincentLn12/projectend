import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FilesApi } from '../../../../core/files/api/files.api';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { ProjectsApi } from '../api/projects.api';
import { ProjectDetail, ProjectListItem } from '../types/project.types';

const OTHER_COUNT = 3;
const YOUTUBE_ID_PATTERN = /(?:youtube\.com\/(?:watch\?v=|embed\/|shorts\/)|youtu\.be\/)([\w-]{11})/;
const FACEBOOK_VIDEO_PATTERN = /^https?:\/\/(?:[\w-]+\.)?(?:facebook\.com|fb\.watch)\//i;

@Component({
  selector: 'app-public-project-detail',
  imports: [RouterLink, Button, DataState, FileImage],
  templateUrl: './project-detail.html',
})
export class PublicProjectDetail implements OnInit {
  private readonly api = inject(ProjectsApi);
  private readonly filesApi = inject(FilesApi);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly projectId = inject(ActivatedRoute).snapshot.paramMap.get('id')!;

  protected readonly project = signal<ProjectDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly otherProjects = signal<ProjectListItem[]>([]);

  protected readonly keywords = computed(() =>
    (this.project()?.keyWords ?? '')
      .split(',')
      .map((word) => word.trim())
      .filter((word) => word !== ''),
  );
  protected readonly videoEmbedUrl = computed<SafeResourceUrl | null>(() => {
    const url = this.project()?.videoUrl?.trim();
    if (!url) {
      return null;
    }
    const youtubeId = url.match(YOUTUBE_ID_PATTERN)?.[1];
    if (youtubeId) {
      return this.sanitizer.bypassSecurityTrustResourceUrl(`https://www.youtube-nocookie.com/embed/${youtubeId}`);
    }
    if (FACEBOOK_VIDEO_PATTERN.test(url)) {
      return this.sanitizer.bypassSecurityTrustResourceUrl(
        `https://www.facebook.com/plugins/video.php?href=${encodeURIComponent(url)}&show_text=false`,
      );
    }
    return null;
  });

  protected readonly advisorNames = computed(() =>
    (this.project()?.advisors ?? []).map((advisor) => advisor.lecturerName).join(' / '),
  );
  protected readonly pdfUrl = computed(() => {
    const id = this.project()?.pdfFileId;
    return id ? this.filesApi.url(id) : null;
  });

  ngOnInit(): void {
    this.load();
    this.api.list({ page: 1, pageSize: OTHER_COUNT + 1, search: '' }, null).subscribe({
      next: (result) => this.otherProjects.set(result.items.filter((item) => item.id !== this.projectId).slice(0, OTHER_COUNT)),
    });
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
