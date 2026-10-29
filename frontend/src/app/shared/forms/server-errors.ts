import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';

interface ProblemBody {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

// Puts the API's validation messages under the matching fields of the form.
// The API names fields in PascalCase (Title) and the form controls use camelCase (title).
// Returns a message for everything that does not belong to a single field, or null when there is nothing left to show.
export function applyServerErrors(form: FormGroup, error: unknown): string | null {
  if (!(error instanceof HttpErrorResponse)) {
    return 'เกิดข้อผิดพลาด กรุณาลองใหม่ภายหลัง';
  }

  if (error.status === 0) {
    return 'เชื่อมต่อระบบไม่ได้ กรุณาลองใหม่ภายหลัง';
  }
  if (error.status === 401 || error.status === 403) {
    return 'ไม่มีสิทธิ์ดำเนินการนี้';
  }
  if (error.status === 429) {
    return 'ทำรายการถี่เกินไป กรุณารอสักครู่แล้วลองใหม่';
  }
  if (error.status >= 500) {
    return 'ระบบขัดข้อง กรุณาลองใหม่ภายหลัง';
  }

  const body = (error.error ?? {}) as ProblemBody;
  let unmatched = false;

  for (const [field, messages] of Object.entries(body.errors ?? {})) {
    const control = form.get(toControlName(field));
    if (control && messages.length > 0) {
      control.setErrors({ server: messages[0] });
      control.markAsTouched();
    } else {
      unmatched = true;
    }
  }

  if (body.errors && !unmatched) {
    return null;
  }
  return body.detail ?? body.title ?? 'ข้อมูลไม่ถูกต้อง';
}

function toControlName(field: string): string {
  return field.charAt(0).toLowerCase() + field.slice(1);
}
