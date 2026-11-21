import { ChangeDetectionStrategy, Component, booleanAttribute, computed, input } from '@angular/core';
import { FormControlBase } from '../../forms/form-control-base';
import { Field } from '../field/field';

// An on/off switch (for example "published"). Works with the keyboard like any button.
// tone="success" draws the switch green when it is on (use it for "published"), tone="info" blue; the default is dark slate.
@Component({
  selector: 'app-toggle',
  imports: [Field],
  templateUrl: './toggle.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Toggle extends FormControlBase<boolean> {
  readonly tone = input<'default' | 'success' | 'info'>('default');
  // boxed puts the switch in a panel that is tinted with the tone while it is on; onHint / offHint replace the hint by state.
  readonly boxed = input(false, { transform: booleanAttribute });
  readonly onHint = input('');
  readonly offHint = input('');

  protected readonly shownHint = computed(() => {
    const stateHint = this.value() === true ? this.onHint() : this.offHint();
    return stateHint || this.hint();
  });

  protected readonly boxClass = computed(() => {
    if (!this.boxed()) {
      return '';
    }
    const tinted = this.value() === true && this.tone() !== 'default';
    const tint = this.tone() === 'info' ? 'border-blue-200 bg-blue-50' : 'border-emerald-200 bg-emerald-50';
    return `rounded-xl border p-3.5 transition-colors ${tinted ? tint : 'border-slate-200 bg-slate-50'}`;
  });

  protected flip(): void {
    this.update(this.value() !== true);
    this.markTouched();
  }
}
