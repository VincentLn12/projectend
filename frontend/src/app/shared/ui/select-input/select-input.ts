import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FormControlBase } from '../../forms/form-control-base';
import { Field } from '../field/field';
import { inputClass, softInputClass } from '../styles';

export interface SelectOption<T> {
  value: T;
  label: string;
}

// A drop-down list. The value can be any type (number, enum, id): the list position is what the browser sees,
// and it is mapped back to the real value here.
@Component({
  selector: 'app-select-input',
  imports: [Field],
  templateUrl: './select-input.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SelectInput<T> extends FormControlBase<T> {
  readonly options = input.required<SelectOption<T>[]>();
  readonly placeholder = input('เลือก…');
  readonly shape = input<'default' | 'soft'>('default');

  protected readonly controlClass = computed(() =>
    this.shape() === 'soft' ? softInputClass(!!this.errorMessage()) : inputClass(!!this.errorMessage()),
  );
  protected readonly selectedIndex = computed(() => this.options().findIndex((option) => option.value === this.value()));

  protected onSelect(event: Event): void {
    const index = Number((event.target as HTMLSelectElement).value);
    this.update(index < 0 ? null : this.options()[index].value);
  }
}
