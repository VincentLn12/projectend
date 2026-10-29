import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FilesApi } from '../../../../core/files/api/files.api';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { ProjectsApi } from '../api/projects.api';
import { ProjectDetail } from '../types/project.types';

const ADVISOR_ROLE_MAIN = 1;

@Component({
  selector: 'app-public-project-detail',
  imports: [RouterLink, Button, DataState, FileImage],
  templateUrl: './project-detail.html',
})
export class PublicProjectDetail implements OnInit {
  private readonly api = inject(ProjectsApi);
  private readonly filesApi = inject(FilesApi);
  private readonly projectId = inject(ActivatedRoute).snapshot.paramMap.get('id')!;

  protected readonly project = signal<ProjectDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly keywords = computed(() =>
    (this.project()?.keyWords ?? '')
      .split(',')
      .map((word) => word.trim())
      .filter((word) => word !== ''),
  );
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

  protected roleLabel(role: number): string {
    return role === ADVISOR_ROLE_MAIN ? 'อาจารย์ที่ปรึกษาหลัก' : 'อาจารย์ที่ปรึกษาร่วม';
  }
}
