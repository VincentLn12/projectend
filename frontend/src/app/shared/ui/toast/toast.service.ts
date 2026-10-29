import { Injectable, signal } from '@angular/core';

export type ToastKind = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  kind: ToastKind;
  message: string;
}

const MAX_VISIBLE = 5;

// Short messages in the corner: this.toast.success('บันทึกแล้ว'). They disappear by themselves
// (errors stay a little longer) or when closed. The messages are drawn by <app-toast-host> in the app root.
@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly toasts = signal<Toast[]>([]);

  private nextId = 1;
  private readonly timers = new Map<number, ReturnType<typeof setTimeout>>();

  success(message: string): void {
    this.show('success', message, 4000);
  }

  info(message: string): void {
    this.show('info', message, 4000);
  }

  error(message: string): void {
    this.show('error', message, 7000);
  }

  dismiss(id: number): void {
    const timer = this.timers.get(id);
    if (timer !== undefined) {
      clearTimeout(timer);
      this.timers.delete(id);
    }
    this.toasts.update((list) => list.filter((toast) => toast.id !== id));
  }

  private show(kind: ToastKind, message: string, milliseconds: number): void {
    const id = this.nextId++;
    this.toasts.update((list) => [...list, { id, kind, message }].slice(-MAX_VISIBLE));
    this.timers.set(
      id,
      setTimeout(() => this.dismiss(id), milliseconds),
    );
  }
}
