import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormControlBase } from '../../forms/form-control-base';
import { Field } from '../field/field';
import { inputClass } from '../styles';

// Several lines of text. With maxLength set it also shows how many characters are used.
@Component({
  selector: 'app-text-area',
  imports: [Field],
  templateUrl: './text-area.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TextArea extends FormControlBase<string> {
  readonly rows = input(4);
  readonly placeholder = input('');
  readonly maxLength = input<number | null>(null);

  protected readonly inputClass = inputClass;

  protected onInput(event: Event): void {
    this.update((event.target as HTMLTextAreaElement).value);
  }

  protected get length(): number {
    return (this.value() ?? '').length;
  }
}
