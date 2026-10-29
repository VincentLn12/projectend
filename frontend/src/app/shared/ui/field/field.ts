import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// The frame around a control: label (with a red star when required), the control itself, then the error or hint.
@Component({
  selector: 'app-field',
  templateUrl: './field.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Field {
  readonly label = input('');
  readonly hint = input('');
  readonly error = input<string | null>(null);
  readonly required = input(false);
  readonly inputId = input('');
}
