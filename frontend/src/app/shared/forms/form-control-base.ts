import { DestroyRef, Directive, afterNextRender, computed, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ControlValueAccessor, NgControl, Validators } from '@angular/forms';
import { firstValidationMessage } from './validation-messages';

let nextUid = 0;

// Shared behaviour of every form component: it plugs into Angular forms (formControl, formControlName, ngModel),
// remembers the value, and works out the label's "required" star and the error text from the control itself.
@Directive()
export abstract class FormControlBase<T> implements ControlValueAccessor {
  readonly label = input('');
  readonly hint = input('');

  protected readonly uid = `app-control-${nextUid++}`;
  protected readonly value = signal<T | null>(null);
  protected readonly isDisabled = signal(false);

  protected readonly ngControl = inject(NgControl, { self: true, optional: true });
  private readonly destroyRef = inject(DestroyRef);

  // Bumped whenever the control's state changes, so the values below are recomputed.
  private readonly controlChanges = signal(0);
  private watching = false;

  private onChange: (value: T | null) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  protected readonly isRequired = computed(() => {
    this.controlChanges();
    return this.ngControl?.control?.hasValidator(Validators.required) ?? false;
  });

  // Shown only after the person has touched or changed the field, never on a fresh empty form.
  protected readonly errorMessage = computed(() => {
    this.controlChanges();
    const control = this.ngControl?.control;
    if (!control || !control.invalid || !(control.touched || control.dirty)) {
      return null;
    }
    return firstValidationMessage(control.errors);
  });

  protected readonly describedBy = computed(() => {
    if (this.errorMessage()) {
      return `${this.uid}-error`;
    }
    return this.hint() ? `${this.uid}-hint` : null;
  });

  constructor() {
    if (this.ngControl) {
      this.ngControl.valueAccessor = this;
    }

    // With formControlName, Angular attaches the control to this component only AFTER it has called
    // registerOnChange, so the control cannot be watched there. By the end of the first render it exists.
    afterNextRender(() => this.watchControl());
  }

  writeValue(value: T | null): void {
    this.value.set(value);
  }

  registerOnChange(fn: (value: T | null) => void): void {
    this.onChange = fn;
    this.watchControl();
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(disabled: boolean): void {
    this.isDisabled.set(disabled);
  }

  protected update(value: T | null): void {
    this.value.set(value);
    this.onChange(value);
  }

  protected markTouched(): void {
    this.onTouched();
  }

  private watchControl(): void {
    const control = this.ngControl?.control;
    if (this.watching || !control) {
      return;
    }
    this.watching = true;
    control.events
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.controlChanges.update((n) => n + 1));

    // The control may have changed before we started listening (for example touched already).
    this.controlChanges.update((n) => n + 1);
  }
}
