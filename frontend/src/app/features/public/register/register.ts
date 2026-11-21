import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { applyServerErrors } from '../../../shared/forms/server-errors';
import { studentCodeValidator } from '../../../shared/forms/validators';
import { Button } from '../../../shared/ui/button/button';
import { Checkbox } from '../../../shared/ui/checkbox/checkbox';
import { SelectInput, SelectOption } from '../../../shared/ui/select-input/select-input';
import { TextInput } from '../../../shared/ui/text-input/text-input';

const PREFIXES = ['นาย', 'นางสาว', 'นาง'];

function acceptedValidator(control: AbstractControl): ValidationErrors | null {
  return control.value === true ? null : { required: true };
}

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, Button, Checkbox, SelectInput, TextInput],
  templateUrl: './register.html',
})
export class Register {
  private readonly auth = inject(AuthService);

  protected readonly prefixOptions: SelectOption<string>[] = PREFIXES.map((prefix) => ({ value: prefix, label: prefix }));

  protected readonly form = inject(FormBuilder).group({
    studentCode: ['', [Validators.required, studentCodeValidator]],
    prefix: ['นาย' as string | null],
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128)]],
    confirmPassword: ['', Validators.required],
    acceptPrivacy: [false, acceptedValidator],
  });

  protected readonly submitting = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly registered = signal(false);


  protected submit(): void {
    if (this.submitting()) {
      return;
    }

    const value = this.form.getRawValue();
    if (value.password !== value.confirmPassword) {
      this.form.controls.confirmPassword.setErrors({ server: 'รหัสผ่านทั้งสองช่องไม่ตรงกัน' });
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.formError.set(null);

    this.auth
      .registerStudent({
        email: (value.email ?? '').trim(),
        fullName: `${value.prefix ?? ''}${(value.firstName ?? '').trim()} ${(value.lastName ?? '').trim()}`.trim(),
        studentCode: (value.studentCode ?? '').trim(),
        password: value.password ?? '',
        acceptPrivacy: value.acceptPrivacy ?? false,
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.registered.set(true);
        },
        error: (error: unknown) => {
          this.submitting.set(false);
          this.formError.set(applyServerErrors(this.form, error));
        },
      });
  }
}
