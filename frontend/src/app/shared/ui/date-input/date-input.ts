import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormControlBase } from '../../forms/form-control-base';
import { Field } from '../field/field';
import { inputClass } from '../styles';

// A date ("2030-01-31") or a date with time ("2030-01-31T09:00", no time zone: see date-time.ts to convert it).
// The value is the text the browser gives, or null when the box is empty.
@Component({
  selector: 'app-date-input',
  imports: [Field],
  templateUrl: './date-input.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DateInput extends FormControlBase<string> {
  readonly type = input<'date' | 'datetime-local'>('date');
  readonly min = input<string | null>(null);
  readonly max = input<string | null>(null);

  protected readonly inputClass = inputClass;

  protected onInput(event: Event): void {
    const text = (event.target as HTMLInputElement).value;
    this.update(text === '' ? null : text);
  }
}
