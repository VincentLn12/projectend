import { ValidationErrors } from '@angular/forms';

// Which error to show first when a control has several.
const PRIORITY = ['server', 'required', 'email', 'url', 'minlength', 'maxlength', 'min', 'max', 'pattern'];

export function firstValidationMessage(errors: ValidationErrors | null): string | null {
  if (!errors) {
    return null;
  }

  const keys = Object.keys(errors).sort((a, b) => rank(a) - rank(b));
  for (const key of keys) {
    const message = describe(key, errors[key]);
    if (message) {
      return message;
    }
  }
  return 'ข้อมูลไม่ถูกต้อง';
}

function rank(key: string): number {
  const index = PRIORITY.indexOf(key);
  return index === -1 ? PRIORITY.length : index;
}

function describe(key: string, detail: unknown): string | null {
  const info = (detail ?? {}) as Record<string, unknown>;

  switch (key) {
    case 'server':
      return typeof detail === 'string' ? detail : 'ข้อมูลไม่ถูกต้อง';
    case 'required':
      return 'กรุณากรอกข้อมูล';
    case 'email':
      return 'รูปแบบอีเมลไม่ถูกต้อง';
    case 'url':
      return 'ลิงก์ต้องขึ้นต้นด้วย http:// หรือ https://';
    case 'minlength':
      return `ต้องมีอย่างน้อย ${info['requiredLength']} ตัวอักษร`;
    case 'maxlength':
      return `ต้องไม่เกิน ${info['requiredLength']} ตัวอักษร`;
    case 'min':
      return `ต้องไม่น้อยกว่า ${info['min']}`;
    case 'max':
      return `ต้องไม่มากกว่า ${info['max']}`;
    case 'pattern':
      return 'รูปแบบไม่ถูกต้อง';
    default:
      return null;
  }
}
