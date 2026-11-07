import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

// Same rule as the API's [Url] attribute: empty is fine, otherwise it must start with http://, https:// or ftp://.
export const urlValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value;
  if (value === null || value === undefined || value === '') {
    return null;
  }
  return /^(https?|ftp):\/\//i.test(String(value)) ? null : { url: true };
};

// Requires at least one item in a list control (for example the files of a multi-file upload).
export const notEmptyListValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value;
  return Array.isArray(value) && value.length > 0 ? null : { required: true };
};

// Student code: exactly 11 digits (the first two are the entry year, for example 67 for 2567).
export const studentCodeValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value;
  if (value === null || value === undefined || value === '') {
    return null;
  }
  return /^\d{11}$/.test(String(value).trim()) ? null : { studentCode: true };
};

// Thai phone number as the API accepts it: 9 or 10 digits starting with 0 (spaces and dashes are ignored).
export const thaiPhoneValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value;
  if (value === null || value === undefined || value === '') {
    return null;
  }
  const digits = String(value).replace(/\D/g, '');
  return /^0\d{8,9}$/.test(digits) ? null : { phone: true };
};
