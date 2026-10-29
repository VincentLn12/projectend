import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Injector,
  afterNextRender,
  effect,
  inject,
  viewChild,
} from '@angular/core';
import { Button } from '../button/button';
import { ConfirmService } from './confirm.service';

// Uses the browser's own <dialog>: it traps the keyboard focus, closes on Esc and dims the page by itself.
@Component({
  selector: 'app-confirm-dialog',
  imports: [Button],
  templateUrl: './confirm-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialog {
  protected readonly confirm = inject(ConfirmService);
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');
  private readonly injector = inject(Injector);

  constructor() {
    effect(() => {
      const wanted = this.confirm.current() !== null;
      const element = this.dialog().nativeElement;

      if (wanted && !element.open) {
        element.showModal();
        // The buttons are drawn after this point, so the browser's own autofocus finds nothing.
        // Put the focus on "cancel" once they exist: the safe answer for a risky question.
        afterNextRender(() => element.querySelector<HTMLElement>('[autofocus]')?.focus(), { injector: this.injector });
      } else if (!wanted && element.open) {
        element.close();
      }
    });
  }

  // Esc
  protected onCancel(event: Event): void {
    event.preventDefault();
    this.confirm.answer(false);
  }

  // A click on the dimmed area lands on the dialog element itself, not on its content.
  protected onBackdropClick(event: MouseEvent): void {
    if (event.target === this.dialog().nativeElement) {
      this.confirm.answer(false);
    }
  }
}
