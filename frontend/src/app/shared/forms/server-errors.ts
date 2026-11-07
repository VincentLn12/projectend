import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';

interface ProblemBody {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

// A short message for a failed request that is not about a form (loading a list, deleting a row...).
export function describeHttpError(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'เกิดข้อผิดพลาด กรุณาลองใหม่ภายหลัง';
  }

  switch (error.status) {
    case 0:
      return 'เชื่อมต่อระบบไม่ได้ กรุณาลองใหม่ภายหลัง';
    case 401:
    case 403:
      return 'ไม่มีสิทธิ์ดำเนินการนี้';
    case 404:
      return 'ไม่พบข้อมูลที่ต้องการ';
    case 409:
      return typeof error.error?.detail === 'string' ? error.error.detail : 'ข้อมูลขัดแย้งกับข้อมูลปัจจุบัน';
    case 429:
      return 'ทำรายการถี่เกินไป กรุณารอสักครู่แล้วลองใหม่';
    default:
      return error.status >= 500 ? 'ระบบขัดข้อง กรุณาลองใหม่ภายหลัง' : 'ดำเนินการไม่สำเร็จ';
  }
}

// Like describeHttpError, but shows the sentence the API wrote when it sent one (for example "this phone number is already registered").
export function describeProblemDetail(error: unknown): string {
  const detail = error instanceof HttpErrorResponse && error.status !== 429 ? error.error?.detail : null;
  return typeof detail === 'string' && detail.length > 0 ? detail : describeHttpError(error);
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
