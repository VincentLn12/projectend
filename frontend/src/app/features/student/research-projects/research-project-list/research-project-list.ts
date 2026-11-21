import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { describeHttpError } from '../../../../shared/forms/server-errors';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { DataState } from '../../../../shared/ui/data-state/data-state';
import { FileImage } from '../../../../shared/ui/file-image/file-image';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { MyResearchProjectsApi } from '../api/research-projects.api';
import { MyResearchProjectListItem } from '../types/research-project.types';

@Component({
  selector: 'app-my-research-project-list',
  imports: [RouterLink, Button, DataState, FileImage],
  templateUrl: './research-project-list.html',
})
export class MyResearchProjectList implements OnInit {
  private readonly api = inject(MyResearchProjectsApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly projects = signal<MyResearchProjectListItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.list().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(describeHttpError(error));
      },
    });
  }

  protected togglePublished(project: MyResearchProjectListItem): void {
    const publish = !project.isPublished;
    this.api.setPublished(project.id, publish).subscribe({
      next: () => {
        this.projects.update((list) => list.map((item) => (item.id === project.id ? { ...item, isPublished: publish } : item)));
        this.toast.success(publish ? 'เผยแพร่โครงงานแล้ว' : 'ซ่อนโครงงานแล้ว');
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }

  protected async askToDelete(project: MyResearchProjectListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบโครงงานวิจัยนี้?',
      message: `"${project.nameTH}" จะถูกลบออกจากเว็บไซต์`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(project.id).subscribe({
      next: () => {
        this.toast.success('ลบโครงงานแล้ว');
        this.projects.update((list) => list.filter((item) => item.id !== project.id));
      },
      error: (error: unknown) => this.toast.error(describeHttpError(error)),
    });
  }
}
