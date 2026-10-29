import { Pipe, PipeTransform } from '@angular/core';

const LONG = new Intl.DateTimeFormat('th-TH-u-ca-buddhist', { day: 'numeric', month: 'long', year: 'numeric' });
const SHORT = new Intl.DateTimeFormat('th-TH-u-ca-buddhist', { day: 'numeric', month: 'short', year: 'numeric' });

// 2026-05-31T11:13:00Z -> "31 พฤษภาคม 2569" (Buddhist year). Empty or invalid values show "—".
//   {{ news.publishedAt | thaiDate }}   {{ news.publishedAt | thaiDate: 'short' }}
@Pipe({ name: 'thaiDate' })
export class ThaiDatePipe implements PipeTransform {
  transform(value: string | Date | null | undefined, style: 'long' | 'short' = 'long'): string {
    if (!value) {
      return '—';
    }
    const date = value instanceof Date ? value : new Date(value);
    if (Number.isNaN(date.getTime())) {
      return '—';
    }
    return (style === 'short' ? SHORT : LONG).format(date);
  }
}
