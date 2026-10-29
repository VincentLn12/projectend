import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { isoToLocalDateTime, localDateTimeToIso } from './date-time';
import { applyServerErrors } from './server-errors';
import { firstValidationMessage } from './validation-messages';
import { notEmptyListValidator, urlValidator } from './validators';

describe('firstValidationMessage', () => {
  it('returns nothing when there are no errors', () => {
    expect(firstValidationMessage(null)).toBeNull();
  });

  it('describes the common errors in Thai, using the numbers from the validator', () => {
    expect(firstValidationMessage({ required: true })).toBe('กรุณากรอกข้อมูล');
    expect(firstValidationMessage({ minlength: { requiredLength: 8, actualLength: 3 } })).toBe('ต้องมีอย่างน้อย 8 ตัวอักษร');
    expect(firstValidationMessage({ maxlength: { requiredLength: 50, actualLength: 60 } })).toBe('ต้องไม่เกิน 50 ตัวอักษร');
    expect(firstValidationMessage({ min: { min: 2500, actual: 1 } })).toBe('ต้องไม่น้อยกว่า 2500');
    expect(firstValidationMessage({ max: { max: 2700, actual: 9999 } })).toBe('ต้องไม่มากกว่า 2700');
  });

  it('shows the most important error first and a message from the server before everything else', () => {
    expect(firstValidationMessage({ maxlength: { requiredLength: 5 }, required: true })).toBe('กรุณากรอกข้อมูล');
    expect(firstValidationMessage({ required: true, server: 'ชื่อนี้ถูกใช้แล้ว' })).toBe('ชื่อนี้ถูกใช้แล้ว');
  });

  it('falls back to a general message for an unknown error', () => {
    expect(firstValidationMessage({ somethingCustom: true })).toBe('ข้อมูลไม่ถูกต้อง');
  });
});

describe('validators', () => {
  it('urlValidator accepts empty values and http(s)/ftp links only', () => {
    const check = (value: unknown) => urlValidator(new FormControl(value));

    expect(check('')).toBeNull();
    expect(check(null)).toBeNull();
    expect(check('https://example.com/a')).toBeNull();
    expect(check('HTTP://example.com')).toBeNull();
    expect(check('ftp://example.com')).toBeNull();
    expect(check('example.com')).toEqual({ url: true });
    expect(check('-')).toEqual({ url: true });
  });

  it('notEmptyListValidator wants at least one item', () => {
    expect(notEmptyListValidator(new FormControl([]))).toEqual({ required: true });
    expect(notEmptyListValidator(new FormControl(['a']))).toBeNull();
  });
});

describe('applyServerErrors', () => {
  const makeForm = () =>
    new FormGroup({
      title: new FormControl('', Validators.required),
      body: new FormControl(''),
    });

  it('puts each API field message under its control (PascalCase -> camelCase)', () => {
    const form = makeForm();
    const error = new HttpErrorResponse({ status: 400, error: { errors: { Title: ['ชื่อซ้ำ'], Body: ['สั้นไป', 'อื่นๆ'] } } });

    const general = applyServerErrors(form, error);

    expect(general).toBeNull();
    expect(form.controls.title.errors).toEqual({ server: 'ชื่อซ้ำ' });
    expect(form.controls.body.errors).toEqual({ server: 'สั้นไป' });
    expect(form.controls.title.touched).toBe(true);
  });

  it('returns the message for problems that belong to no single field', () => {
    const form = makeForm();
    const error = new HttpErrorResponse({ status: 400, error: { title: 'Bad request', detail: 'Some files do not exist.' } });

    expect(applyServerErrors(form, error)).toBe('Some files do not exist.');
    expect(form.controls.title.errors).toEqual({ required: true });
  });

  it('keeps the message for unmatched fields even when others matched', () => {
    const form = makeForm();
    const error = new HttpErrorResponse({ status: 400, error: { errors: { Title: ['x'], Unknown: ['y'] }, detail: 'ข้อมูลไม่ถูกต้อง' } });

    expect(applyServerErrors(form, error)).toBe('ข้อมูลไม่ถูกต้อง');
  });

  it('gives a friendly message for network, permission, rate-limit and server failures', () => {
    const form = makeForm();
    const message = (status: number) => applyServerErrors(form, new HttpErrorResponse({ status }));

    expect(message(0)).toContain('เชื่อมต่อระบบไม่ได้');
    expect(message(403)).toContain('ไม่มีสิทธิ์');
    expect(message(429)).toContain('ถี่เกินไป');
    expect(message(500)).toContain('ระบบขัดข้อง');
  });

  it('the server message disappears when the person edits the field', () => {
    const form = makeForm();
    applyServerErrors(form, new HttpErrorResponse({ status: 400, error: { errors: { Title: ['ชื่อซ้ำ'] } } }));

    form.controls.title.setValue('ชื่อใหม่');

    expect(form.controls.title.errors).toBeNull();
  });
});

describe('date-time helpers', () => {
  it('converts what a datetime-local input gives to an ISO time and back', () => {
    const iso = localDateTimeToIso('2030-01-31T09:30');

    expect(iso).toBe(new Date('2030-01-31T09:30').toISOString());
    expect(isoToLocalDateTime(iso)).toBe('2030-01-31T09:30');
  });

  it('treats an API time with no zone as UTC', () => {
    const asUtc = isoToLocalDateTime('2030-01-31T02:00:00');

    expect(asUtc).toBe(isoToLocalDateTime('2030-01-31T02:00:00Z'));
  });

  it('returns null for empty or broken values', () => {
    expect(localDateTimeToIso('')).toBeNull();
    expect(localDateTimeToIso('not a date')).toBeNull();
    expect(isoToLocalDateTime(null)).toBeNull();
    expect(isoToLocalDateTime('nonsense')).toBeNull();
  });
});
