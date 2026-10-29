import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  Checkbox,
  DateInput,
  SelectInput,
  SelectOption,
  TextArea,
  TextInput,
  Toggle,
} from './index';

async function render<T>(host: new () => T): Promise<ComponentFixture<T>> {
  const fixture = TestBed.createComponent(host);
  fixture.detectChanges();
  await fixture.whenStable();
  return fixture;
}

function query<E extends Element>(fixture: ComponentFixture<unknown>, selector: string): E {
  return fixture.nativeElement.querySelector(selector) as E;
}

function typeInto(element: HTMLInputElement | HTMLTextAreaElement, text: string): void {
  element.value = text;
  element.dispatchEvent(new Event('input'));
}

// ---------------------------------------------------------------------------------------------------------------------

@Component({
  imports: [ReactiveFormsModule, TextInput],
  template: `<app-text-input
    label="ชื่อ"
    hint="ใส่ชื่อจริง"
    [type]="type"
    [formControl]="control"
  />`,
})
class TextHost {
  type: 'text' | 'password' | 'number' = 'text';
  control = new FormControl<string | number | null>('', {
    validators: [Validators.required, Validators.minLength(3)],
  });
}

describe('TextInput', () => {
  it('shows the label with a star for a required field, and the hint', async () => {
    const fixture = await render(TextHost);

    expect(query(fixture, 'label').textContent).toContain('ชื่อ');
    expect(query(fixture, 'label').textContent).toContain('*');
    expect(fixture.nativeElement.textContent).toContain('ใส่ชื่อจริง');
  });

  it('shows what the form holds, and sends what the person types back to the form', async () => {
    const fixture = await render(TextHost);
    fixture.componentInstance.control.setValue('สมชาย');
    fixture.detectChanges();

    expect(query<HTMLInputElement>(fixture, 'input').value).toBe('สมชาย');

    typeInto(query<HTMLInputElement>(fixture, 'input'), 'สมหญิง');
    expect(fixture.componentInstance.control.value).toBe('สมหญิง');
    expect(fixture.componentInstance.control.dirty).toBe(true);
  });

  it('shows no error on a fresh form, then shows it once the person leaves the field', async () => {
    const fixture = await render(TextHost);
    expect(query(fixture, '[role="alert"]')).toBeNull();

    query<HTMLInputElement>(fixture, 'input').dispatchEvent(new Event('blur'));
    fixture.detectChanges();
    await fixture.whenStable();

    expect(query(fixture, '[role="alert"]').textContent).toContain('กรุณากรอกข้อมูล');
    expect(query(fixture, 'input').getAttribute('aria-invalid')).toBe('true');
  });

  it('shows errors when the form is marked as touched from code (for example on submit)', async () => {
    const fixture = await render(TextHost);

    fixture.componentInstance.control.markAsTouched();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(query(fixture, '[role="alert"]')).not.toBeNull();
  });

  it('shows the length rule once something is typed', async () => {
    const fixture = await render(TextHost);

    typeInto(query<HTMLInputElement>(fixture, 'input'), 'ab');
    fixture.detectChanges();
    await fixture.whenStable();

    expect(query(fixture, '[role="alert"]').textContent).toContain('ต้องมีอย่างน้อย 3 ตัวอักษร');
  });

  it('is disabled when the control is disabled', async () => {
    const fixture = await render(TextHost);

    fixture.componentInstance.control.disable();
    fixture.detectChanges();

    expect(query<HTMLInputElement>(fixture, 'input').disabled).toBe(true);
  });

  it('password: the show/hide button switches the input type', async () => {
    const fixture = TestBed.createComponent(TextHost);
    fixture.componentInstance.type = 'password';
    fixture.detectChanges();
    await fixture.whenStable();

    expect(query<HTMLInputElement>(fixture, 'input').type).toBe('password');

    query<HTMLButtonElement>(fixture, 'button').click();
    fixture.detectChanges();

    expect(query<HTMLInputElement>(fixture, 'input').type).toBe('text');
  });

  it('number: the form gets a number, and null when the box is emptied', async () => {
    const fixture = TestBed.createComponent(TextHost);
    fixture.componentInstance.type = 'number';
    fixture.detectChanges();
    await fixture.whenStable();

    typeInto(query<HTMLInputElement>(fixture, 'input'), '2569');
    expect(fixture.componentInstance.control.value).toBe(2569);

    typeInto(query<HTMLInputElement>(fixture, 'input'), '');
    expect(fixture.componentInstance.control.value).toBeNull();
  });
});

// ---------------------------------------------------------------------------------------------------------------------
// The real forms use formControlName inside a [formGroup]. Angular connects the control to the component
// later than it does for [formControl], which once made the errors never appear in the browser.

@Component({
  imports: [ReactiveFormsModule, TextInput, SelectInput],
  template: `
    <form [formGroup]="form">
      <app-text-input label="ชื่อ" formControlName="name" />
      <app-select-input label="ประเภท" formControlName="kind" [options]="kinds" />
    </form>
  `,
})
class GroupHost {
  kinds: SelectOption<number>[] = [{ value: 1, label: 'หนึ่ง' }];
  form = new FormGroup({
    name: new FormControl('', Validators.required),
    kind: new FormControl<number | null>(null, Validators.required),
  });
}

describe('components inside a formGroup (formControlName)', () => {
  it('show the required star', async () => {
    const fixture = await render(GroupHost);

    expect(fixture.nativeElement.querySelectorAll('label')[0].textContent).toContain('*');
  });

  it('show every error after the whole form is marked as touched from code', async () => {
    const fixture = await render(GroupHost);
    expect(fixture.nativeElement.querySelectorAll('[role="alert"]').length).toBe(0);

    fixture.componentInstance.form.markAllAsTouched();
    fixture.detectChanges();
    await fixture.whenStable();

    const messages = Array.from(fixture.nativeElement.querySelectorAll('[role="alert"]')).map((a) =>
      (a as HTMLElement).textContent?.trim(),
    );
    expect(messages).toEqual(['กรุณากรอกข้อมูล', 'กรุณากรอกข้อมูล']);
  });

  it('show the error once the person leaves an empty field', async () => {
    const fixture = await render(GroupHost);

    fixture.nativeElement.querySelector('input').dispatchEvent(new Event('blur'));
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelectorAll('[role="alert"]').length).toBe(1);
  });

  it('show a server error set on a control', async () => {
    const fixture = await render(GroupHost);

    fixture.componentInstance.form.controls.name.setErrors({ server: 'ชื่อนี้ถูกใช้แล้ว' });
    fixture.componentInstance.form.controls.name.markAsTouched();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain(
      'ชื่อนี้ถูกใช้แล้ว',
    );
  });
});

// ---------------------------------------------------------------------------------------------------------------------

@Component({
  imports: [ReactiveFormsModule, TextArea],
  template: `<app-text-area label="รายละเอียด" [maxLength]="20" [formControl]="control" />`,
})
class AreaHost {
  control = new FormControl('');
}

describe('TextArea', () => {
  it('passes text both ways and counts the characters', async () => {
    const fixture = await render(AreaHost);
    fixture.componentInstance.control.setValue('สวัสดี');
    fixture.detectChanges();
    await fixture.whenStable();

    expect(query<HTMLTextAreaElement>(fixture, 'textarea').value).toBe('สวัสดี');
    expect(fixture.nativeElement.textContent).toContain('6/20');

    typeInto(query<HTMLTextAreaElement>(fixture, 'textarea'), 'abc');
    expect(fixture.componentInstance.control.value).toBe('abc');
  });
});

// ---------------------------------------------------------------------------------------------------------------------

@Component({
  imports: [ReactiveFormsModule, SelectInput],
  template: `<app-select-input
    label="ประเภท"
    placeholder="เลือกประเภท"
    [options]="options"
    [formControl]="control"
  />`,
})
class SelectHost {
  options: SelectOption<number>[] = [
    { value: 10, label: 'สิบ' },
    { value: 20, label: 'ยี่สิบ' },
  ];
  control = new FormControl<number | null>(null, Validators.required);
}

@Component({
  imports: [ReactiveFormsModule, SelectInput],
  template: `<app-select-input
    label="อาจารย์"
    placeholder="เลือกอาจารย์"
    [options]="options"
    [searchable]="true"
    [formControl]="control"
  />`,
})
class SearchableSelectHost {
  options: SelectOption<string>[] = [
    { value: 'a', label: 'สมชาย ใจดี' },
    { value: 'b', label: 'Somying Rakngan' },
    { value: 'c', label: 'สมศักดิ์ มั่นคง' },
  ];
  control = new FormControl<string | null>(null);
}

@Component({
  imports: [ReactiveFormsModule, SelectInput],
  template: `<app-select-input label="ปี" [options]="options" [formControl]="control" />`,
})
class LongSelectHost {
  options: SelectOption<number>[] = Array.from({ length: 8 }, (_, index) => ({ value: index, label: `ตัวเลือก ${index}` }));
  control = new FormControl<number | null>(null);
}

describe('SelectInput', () => {
  const choose = (fixture: ComponentFixture<unknown>, index: number) => {
    const select = query<HTMLSelectElement>(fixture, 'select');
    select.value = String(index);
    select.dispatchEvent(new Event('change'));
  };

  it('lists the placeholder first, then the options', async () => {
    const fixture = await render(SelectHost);
    const labels = Array.from(fixture.nativeElement.querySelectorAll('option')).map((o) =>
      (o as HTMLOptionElement).textContent?.trim(),
    );

    expect(labels).toEqual(['เลือกประเภท', 'สิบ', 'ยี่สิบ']);
  });

  it('gives the form the real value (a number here), not the list position', async () => {
    const fixture = await render(SelectHost);

    choose(fixture, 1);

    expect(fixture.componentInstance.control.value).toBe(20);
  });

  it('shows the option that matches the form value, and the placeholder for null', async () => {
    const fixture = await render(SelectHost);
    const select = query<HTMLSelectElement>(fixture, 'select');

    fixture.componentInstance.control.setValue(10);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(select.selectedIndex).toBe(1);

    fixture.componentInstance.control.setValue(null);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(select.selectedIndex).toBe(0);
  });

  it('choosing the placeholder clears the value', async () => {
    const fixture = await render(SelectHost);
    choose(fixture, 0);
    expect(fixture.componentInstance.control.value).toBe(10);

    choose(fixture, -1);
    expect(fixture.componentInstance.control.value).toBeNull();
  });
});

// ---------------------------------------------------------------------------------------------------------------------

@Component({
  imports: [ReactiveFormsModule, Checkbox, Toggle],
  template: `
    <app-checkbox label="ยอมรับ" [formControl]="accepted" />
    <app-toggle label="เผยแพร่" [formControl]="published" />
  `,
})
class SwitchesHost {
  accepted = new FormControl(false, Validators.requiredTrue);
  published = new FormControl(true);
}

describe('Checkbox and Toggle', () => {
  it('checkbox reflects and changes the form value', async () => {
    const fixture = await render(SwitchesHost);
    const box = query<HTMLInputElement>(fixture, 'input[type="checkbox"]');
    expect(box.checked).toBe(false);

    box.click();
    fixture.detectChanges(); // in the app the screen is redrawn after every click
    expect(fixture.componentInstance.accepted.value).toBe(true);
    expect(fixture.componentInstance.accepted.touched).toBe(true);

    fixture.componentInstance.accepted.setValue(false);
    fixture.detectChanges();
    expect(box.checked).toBe(false);
  });

  it('toggle starts from the form value and flips on click', async () => {
    const fixture = await render(SwitchesHost);
    const toggle = query<HTMLButtonElement>(fixture, '[role="switch"]');
    expect(toggle.getAttribute('aria-checked')).toBe('true');

    toggle.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.published.value).toBe(false);
    expect(toggle.getAttribute('aria-checked')).toBe('false');
  });

  it('toggle cannot be used when the control is disabled', async () => {
    const fixture = await render(SwitchesHost);

    fixture.componentInstance.published.disable();
    fixture.detectChanges();

    expect(query<HTMLButtonElement>(fixture, '[role="switch"]').disabled).toBe(true);
  });
});

// ---------------------------------------------------------------------------------------------------------------------

@Component({
  imports: [ReactiveFormsModule, DateInput],
  template: `<app-date-input label="วันที่" [formControl]="control" />`,
})
class DateHost {
  control = new FormControl<string | null>(null);
}

@Component({
  imports: [ReactiveFormsModule, DateInput],
  template: `<app-date-input label="วันที่" min="2030-01-10" max="2030-01-20" [formControl]="control" />`,
})
class DateLimitHost {
  control = new FormControl<string | null>('2030-01-15');
}

@Component({
  imports: [ReactiveFormsModule, DateInput],
  template: `<app-date-input label="วันและเวลา" type="datetime-local" [formControl]="control" />`,
})
class DateTimeHost {
  control = new FormControl<string | null>(null);
}

describe('DateInput', () => {
  const openCalendar = (fixture: ComponentFixture<unknown>) => {
    query<HTMLButtonElement>(fixture, 'button[aria-haspopup="dialog"]').click();
    fixture.detectChanges();
  };

  it('shows a placeholder, then the chosen date in Thai with a Buddhist year', async () => {
    const fixture = await render(DateHost);
    const trigger = query<HTMLButtonElement>(fixture, 'button[aria-haspopup="dialog"]');
    expect(trigger.textContent?.trim()).toBe('เลือกวันที่');

    fixture.componentInstance.control.setValue('2031-02-03');
    fixture.detectChanges();

    expect(trigger.textContent?.trim()).toBe('3 กุมภาพันธ์ 2574');
  });

  it('opens a Thai calendar on the month of the current value', async () => {
    const fixture = await render(DateHost);
    fixture.componentInstance.control.setValue('2030-01-31');
    fixture.detectChanges();

    openCalendar(fixture);

    expect(query(fixture, '[role="dialog"]').textContent).toContain('มกราคม 2573');
    expect(query(fixture, '[role="dialog"]').textContent).toContain('พฤ');
  });

  it('picks a day, passes the ISO date to the form and closes', async () => {
    const fixture = await render(DateHost);
    fixture.componentInstance.control.setValue('2030-01-15');
    fixture.detectChanges();
    openCalendar(fixture);

    query<HTMLButtonElement>(fixture, '[aria-label="20 มกราคม 2573"]').click();
    fixture.detectChanges();

    expect(fixture.componentInstance.control.value).toBe('2030-01-20');
    expect(query(fixture, '[role="dialog"]')).toBeNull();
  });

  it('moves between months and clears the date', async () => {
    const fixture = await render(DateHost);
    fixture.componentInstance.control.setValue('2030-01-15');
    fixture.detectChanges();
    openCalendar(fixture);

    query<HTMLButtonElement>(fixture, '[aria-label="เดือนถัดไป"]').click();
    fixture.detectChanges();
    expect(query(fixture, '[role="dialog"]').textContent).toContain('กุมภาพันธ์ 2573');

    const clear = Array.from(fixture.nativeElement.querySelectorAll('[role="dialog"] button')).find(
      (button) => (button as HTMLElement).textContent?.trim() === 'ล้างวันที่',
    ) as HTMLButtonElement;
    clear.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.control.value).toBeNull();
  });

  it('does not let a day outside min and max be picked', async () => {
    const fixture = await render(DateLimitHost);
    openCalendar(fixture);

    const tooEarly = query<HTMLButtonElement>(fixture, '[aria-label="5 มกราคม 2573"]');
    expect(tooEarly.disabled).toBe(true);
    expect(query<HTMLButtonElement>(fixture, '[aria-label="15 มกราคม 2573"]').disabled).toBe(false);
  });

  it('closes on Escape', async () => {
    const fixture = await render(DateHost);
    openCalendar(fixture);

    query(fixture, 'app-date-input').dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    fixture.detectChanges();

    expect(query(fixture, '[role="dialog"]')).toBeNull();
  });

  it('a date with time still uses the browser control', async () => {
    const fixture = await render(DateTimeHost);
    const input = query<HTMLInputElement>(fixture, 'input');
    expect(input.type).toBe('datetime-local');

    typeInto(input, '2030-01-31T09:30');
    expect(fixture.componentInstance.control.value).toBe('2030-01-31T09:30');

    typeInto(input, '');
    expect(fixture.componentInstance.control.value).toBeNull();
  });
});

describe('SelectInput with search', () => {
  const open = async (fixture: ComponentFixture<unknown>) => {
    query<HTMLButtonElement>(fixture, 'button[role="combobox"]').click();
    fixture.detectChanges();
    await fixture.whenStable();
  };
  const type = (fixture: ComponentFixture<unknown>, text: string) => {
    const box = query<HTMLInputElement>(fixture, '[role="listbox"]').parentElement!.querySelector('input') as HTMLInputElement;
    box.value = text;
    box.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  };
  const optionLabels = (fixture: ComponentFixture<unknown>) =>
    Array.from(fixture.nativeElement.querySelectorAll('[role="option"]')).map((o) => (o as HTMLElement).textContent?.replace('✓', '').trim());

  it('shows a button instead of the browser select, listing the placeholder and every option', async () => {
    const fixture = await render(SearchableSelectHost);
    expect(query(fixture, 'select')).toBeNull();

    await open(fixture);

    expect(optionLabels(fixture)).toEqual(['เลือกอาจารย์', 'สมชาย ใจดี', 'Somying Rakngan', 'สมศักดิ์ มั่นคง']);
  });

  it('narrows the list while typing, ignoring upper and lower case', async () => {
    const fixture = await render(SearchableSelectHost);
    await open(fixture);

    type(fixture, 'สมศ');
    expect(optionLabels(fixture)).toEqual(['สมศักดิ์ มั่นคง']);

    type(fixture, 'SOMYING');
    expect(optionLabels(fixture)).toEqual(['Somying Rakngan']);

    type(fixture, 'ไม่มีชื่อนี้');
    expect(optionLabels(fixture)).toEqual([]);
    expect(fixture.nativeElement.textContent).toContain('ไม่พบตัวเลือกที่ตรงกัน');
  });

  it('picks the clicked option, shows its label and closes', async () => {
    const fixture = await render(SearchableSelectHost);
    await open(fixture);

    (fixture.nativeElement.querySelectorAll('[role="option"]')[2] as HTMLElement).click();
    fixture.detectChanges();

    expect(fixture.componentInstance.control.value).toBe('b');
    expect(query(fixture, 'button[role="combobox"]').textContent?.trim()).toBe('Somying Rakngan');
    expect(query(fixture, '[role="listbox"]')).toBeNull();
  });

  it('picks with the arrow keys and Enter, and the placeholder row clears the value', async () => {
    const fixture = await render(SearchableSelectHost);
    fixture.componentInstance.control.setValue('c');
    fixture.detectChanges();
    await open(fixture);

    const box = query<HTMLInputElement>(fixture, 'input');
    box.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowUp', bubbles: true }));
    box.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }));
    fixture.detectChanges();
    expect(fixture.componentInstance.control.value).toBe('b');

    await open(fixture);
    (fixture.nativeElement.querySelectorAll('[role="option"]')[0] as HTMLElement).click();
    fixture.detectChanges();
    expect(fixture.componentInstance.control.value).toBeNull();
  });

  it('closes on Escape', async () => {
    const fixture = await render(SearchableSelectHost);
    await open(fixture);

    query(fixture, 'app-select-input').dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    fixture.detectChanges();

    expect(query(fixture, '[role="listbox"]')).toBeNull();
  });

  it('a list of 8 or more options is searchable without asking, a short one is not', async () => {
    const longList = await render(LongSelectHost);
    expect(query(longList, 'button[role="combobox"]')).not.toBeNull();

    const shortList = await render(SelectHost);
    expect(query(shortList, 'select')).not.toBeNull();
  });
});
