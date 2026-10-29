import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormControlBase } from '../../forms/form-control-base';
import { Field } from '../field/field';

// An on/off switch (for example "published"). Works with the keyboard like any button.
@Component({
  selector: 'app-toggle',
  imports: [Field],
  templateUrl: './toggle.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Toggle extends FormControlBase<boolean> {
  protected flip(): void {
    this.update(this.value() !== true);
    this.markTouched();
  }
}
