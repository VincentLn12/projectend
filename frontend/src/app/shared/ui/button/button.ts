import { ChangeDetectionStrategy, Component, booleanAttribute, computed, input } from '@angular/core';

export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'ghost' | 'accent' | 'red';
export type ButtonSize = 'sm' | 'md';

const BASE =
  'inline-flex items-center justify-center rounded-md font-medium transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-1 disabled:cursor-not-allowed disabled:opacity-60';

const VARIANTS: Record<ButtonVariant, string> = {
  primary: 'bg-brand-800 text-white hover:bg-brand-700 focus-visible:ring-brand-700',
  secondary: 'border border-slate-400 bg-white text-slate-900 hover:bg-slate-100 focus-visible:ring-slate-400',
  danger: 'bg-red-600 text-white hover:bg-red-700 focus-visible:ring-red-300',
  ghost: 'text-brand-800 hover:bg-brand-900/10 focus-visible:ring-brand-700',
  red: 'bg-red-600 font-semibold text-white shadow-sm hover:bg-red-700 focus-visible:ring-red-300',
  accent: 'bg-gold-400 font-semibold text-brand-950 shadow-sm hover:bg-gold-300 focus-visible:ring-gold-300',
};

const SIZES: Record<ButtonSize, string> = {
  sm: 'px-3 py-1 text-sm',
  md: 'px-4 py-2 text-sm',
};

// Put the attribute on a real <button> or <a>, so it keeps working like one (type, disabled, routerLink, keyboard):
//   <button app-button variant="danger" [loading]="saving()" (click)="save()">บันทึก</button>
// While loading it shows a spinner and cannot be clicked again.
@Component({
  selector: 'button[app-button], a[app-button]',
  templateUrl: './button.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': 'classes()',
    '[attr.aria-busy]': 'loading() ? "true" : null',
    '[attr.disabled]': 'disabled() || loading() ? "" : null',
  },
})
export class Button {
  readonly variant = input<ButtonVariant>('primary');
  readonly size = input<ButtonSize>('md');
  readonly loading = input(false);

  // Declared here (not left to the browser) so that [disabled]="..." or a plain `disabled` attribute on the host
  // is combined with the loading state, instead of being overwritten by it.
  readonly disabled = input(false, { transform: booleanAttribute });

  protected readonly classes = computed(() => `${BASE} ${VARIANTS[this.variant()]} ${SIZES[this.size()]}`);
}
