import { Injectable, signal } from '@angular/core';

export interface ConfirmOptions {
  title: string;
  message?: string;
  confirmText?: string;
  cancelText?: string;
  // Red confirm button, for things that cannot be undone.
  danger?: boolean;
}

export interface ConfirmRequest {
  options: ConfirmOptions;
  resolve: (confirmed: boolean) => void;
}

// Ask before doing something risky:
//   if (await this.confirm.ask({ title: 'ลบข่าวนี้?', danger: true })) { ... }
// The dialog itself is <app-confirm-dialog>, placed once in the app root.
@Injectable({ providedIn: 'root' })
export class ConfirmService {
  readonly current = signal<ConfirmRequest | null>(null);

  ask(options: ConfirmOptions): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      this.current()?.resolve(false); // a question that is still open counts as "no"
      this.current.set({ options, resolve });
    });
  }

  answer(confirmed: boolean): void {
    const request = this.current();
    if (!request) {
      return;
    }
    this.current.set(null);
    request.resolve(confirmed);
  }
}
