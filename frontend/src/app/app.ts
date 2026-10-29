import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ConfirmDialog } from './shared/ui/confirm-dialog/confirm-dialog';
import { ToastHost } from './shared/ui/toast/toast-host';

// The confirm dialog and the toast messages live here, once, so any page can use ConfirmService and ToastService.
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ConfirmDialog, ToastHost],
  template: `
    <router-outlet />
    <app-confirm-dialog />
    <app-toast-host />
  `,
})
export class App {}
