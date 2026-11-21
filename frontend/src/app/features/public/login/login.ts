import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { Button } from '../../../shared/ui/button/button';
import { TextInput } from '../../../shared/ui/text-input/text-input';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, TextInput, Button],
  templateUrl: './login.html',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    identifier: ['', Validators.required],
    password: ['', Validators.required],
  });

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const { identifier, password } = this.form.getRawValue();
    this.auth.login(identifier, password).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        this.router.navigateByUrl(returnUrl ?? (this.auth.isAdmin() ? '/admin' : '/'));
      },
      error: (error: unknown) => {
        this.submitting.set(false);
        this.errorMessage.set(this.messageFor(error));
      },
    });
  }

  private messageFor(error: unknown): string {
    const status = error instanceof HttpErrorResponse ? error.status : 0;

    if (status === 401) {
      return 'ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง';
    }
    if (status === 429) {
      return 'ลองเข้าสู่ระบบบ่อยเกินไป กรุณารอสักครู่แล้วลองใหม่';
    }
    return 'เข้าสู่ระบบไม่สำเร็จ กรุณาลองใหม่ภายหลัง';
  }
}
