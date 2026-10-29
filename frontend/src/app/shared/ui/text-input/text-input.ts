import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { FormControlBase } from '../../forms/form-control-base';
import { Field } from '../field/field';
import { inputClass } from '../styles';

export type TextInputType = 'text' | 'email' | 'password' | 'search' | 'tel' | 'url' | 'number';

// One-line text. The value is a string, except for type="number" where it is a number (or null when empty).
@Component({
  selector: 'app-text-input',
  imports: [Field],
  templateUrl: './text-input.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TextInput extends FormControlBase<string | number> {
  readonly type = input<TextInputType>('text');
  readonly placeholder = input('');
  readonly autocomplete = input<string | null>(null);
  readonly maxLength = input<number | null>(null);

  protected readonly inputClass = inputClass;
  protected readonly showPassword = signal(false);
  protected readonly inputType = computed(() => (this.type() === 'password' && this.showPassword() ? 'text' : this.type()));

  protected onInput(event: Event): void {
    const text = (event.target as HTMLInputElement).value;

    if (this.type() === 'number') {
      this.update(text === '' ? null : Number(text));
    } else {
      this.update(text);
    }
  }

  protected togglePassword(): void {
    this.showPassword.update((shown) => !shown);
  }
}
