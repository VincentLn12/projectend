import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormControlBase } from '../../forms/form-control-base';
import { Field } from '../field/field';

// A tick box. The label is the text beside the box.
@Component({
  selector: 'app-checkbox',
  imports: [Field],
  templateUrl: './checkbox.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Checkbox extends FormControlBase<boolean> {
  protected onChecked(event: Event): void {
    this.update((event.target as HTMLInputElement).checked);
    this.markTouched();
  }
}
