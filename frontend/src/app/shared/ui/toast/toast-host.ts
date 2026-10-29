import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastKind, ToastService } from './toast.service';

const STYLES: Record<ToastKind, string> = {
  success: 'border-green-200 bg-green-50 text-green-900',
  info: 'border-slate-200 bg-white text-slate-800',
  error: 'border-red-200 bg-red-50 text-red-900',
};

@Component({
  selector: 'app-toast-host',
  templateUrl: './toast-host.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastHost {
  protected readonly toastService = inject(ToastService);

  protected styleFor(kind: ToastKind): string {
    return STYLES[kind];
  }
}
