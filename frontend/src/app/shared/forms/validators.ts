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
