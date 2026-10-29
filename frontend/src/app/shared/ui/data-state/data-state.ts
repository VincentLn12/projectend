import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Button } from '../button/button';

// Shows what the person should see while data is on its way: a spinner, an error with a retry button,
// an "empty" message, or (when all is well) the content placed inside it.
@Component({
  selector: 'app-data-state',
  imports: [Button],
  templateUrl: './data-state.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataState {
  readonly loading = input(false);
  readonly error = input<string | null>(null);
  readonly empty = input(false);
  readonly loadingMessage = input('กำลังโหลด…');
  readonly emptyMessage = input('ไม่มีข้อมูล');
  readonly retry = output<void>();
}
