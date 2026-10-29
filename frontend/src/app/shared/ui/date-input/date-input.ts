import { ChangeDetectionStrategy, Component, ElementRef, computed, inject, input, signal } from '@angular/core';
import { FormControlBase } from '../../forms/form-control-base';
import { Field } from '../field/field';
import { inputClass } from '../styles';

const LONG_DATE = new Intl.DateTimeFormat('th-TH-u-ca-buddhist', { day: 'numeric', month: 'long', year: 'numeric' });
const MONTH_AND_YEAR = new Intl.DateTimeFormat('th-TH-u-ca-buddhist', { month: 'long', year: 'numeric' });

const WEEKDAY_NAMES = ['อา', 'จ', 'อ', 'พ', 'พฤ', 'ศ', 'ส'];
const WEEKS_SHOWN = 6;
const DAYS_PER_WEEK = 7;

interface CalendarDay {
  iso: string;
  dayOfMonth: number;
  inViewedMonth: boolean;
  disabled: boolean;
  selected: boolean;
  today: boolean;
  label: string;
}

function toIsoDate(date: Date): string {
  const year = String(date.getFullYear()).padStart(4, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function fromIsoDate(text: string | null): Date | null {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(text ?? '');
  if (!match) {
    return null;
  }
  return new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]));
}

// A date ("2030-01-31") or a date with time ("2030-01-31T09:00", no time zone: see date-time.ts to convert it).
// The value is the text the browser gives, or null when the box is empty.
// A date (type "date") is picked on a Thai calendar (Buddhist year, Thai month names) instead of the browser's own
// picker, which follows the browser language. A date with time still uses the browser's control.
@Component({
  selector: 'app-date-input',
  imports: [Field],
  templateUrl: './date-input.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:click)': 'closeWhenClickedOutside($event)',
    '(keydown.escape)': 'close()',
  },
})
export class DateInput extends FormControlBase<string> {
  readonly type = input<'date' | 'datetime-local'>('date');
  readonly min = input<string | null>(null);
  readonly max = input<string | null>(null);

  protected readonly inputClass = inputClass;
  protected readonly weekdayNames = WEEKDAY_NAMES;
  protected readonly isOpen = signal(false);

  private readonly hostElement = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly viewedMonth = signal(new Date(new Date().getFullYear(), new Date().getMonth(), 1));

  protected readonly displayText = computed(() => {
    const date = fromIsoDate(this.value());
    return date ? LONG_DATE.format(date) : 'เลือกวันที่';
  });

  protected readonly monthTitle = computed(() => MONTH_AND_YEAR.format(this.viewedMonth()));

  protected readonly calendarDays = computed<CalendarDay[]>(() => {
    const viewed = this.viewedMonth();
    const firstOfMonth = new Date(viewed.getFullYear(), viewed.getMonth(), 1);
    const selectedIso = this.value();
    const todayIso = toIsoDate(new Date());
    const earliest = this.min();
    const latest = this.max();

    const days: CalendarDay[] = [];
    for (let position = 0; position < WEEKS_SHOWN * DAYS_PER_WEEK; position++) {
      const date = new Date(firstOfMonth.getFullYear(), firstOfMonth.getMonth(), 1 - firstOfMonth.getDay() + position);
      const iso = toIsoDate(date);
      days.push({
        iso,
        dayOfMonth: date.getDate(),
        inViewedMonth: date.getMonth() === viewed.getMonth(),
        disabled: (earliest !== null && iso < earliest) || (latest !== null && iso > latest),
        selected: iso === selectedIso,
        today: iso === todayIso,
        label: LONG_DATE.format(date),
      });
    }
    return days;
  });

  protected onInput(event: Event): void {
    const text = (event.target as HTMLInputElement).value;
    this.update(text === '' ? null : text);
  }

  protected toggleCalendar(): void {
    if (this.isOpen()) {
      this.close();
      return;
    }

    const selected = fromIsoDate(this.value());
    const monthToShow = selected ?? new Date();
    this.viewedMonth.set(new Date(monthToShow.getFullYear(), monthToShow.getMonth(), 1));
    this.isOpen.set(true);
  }

  protected moveMonths(months: number): void {
    const current = this.viewedMonth();
    this.viewedMonth.set(new Date(current.getFullYear(), current.getMonth() + months, 1));
  }

  protected pickDay(day: CalendarDay): void {
    if (day.disabled) {
      return;
    }
    this.update(day.iso);
    this.close();
  }

  protected clearDate(): void {
    this.update(null);
    this.close();
  }

  protected close(): void {
    if (!this.isOpen()) {
      return;
    }
    this.isOpen.set(false);
    this.markTouched();
  }

  protected closeWhenClickedOutside(event: Event): void {
    if (this.isOpen() && !this.hostElement.nativeElement.contains(event.target as Node)) {
      this.close();
    }
  }
}
