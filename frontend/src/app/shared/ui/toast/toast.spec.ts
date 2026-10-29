import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { ToastHost } from './toast-host';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    service = TestBed.inject(ToastService);
  });
  afterEach(() => vi.useRealTimers());

  it('adds messages in order', () => {
    service.success('บันทึกแล้ว');
    service.error('ล้มเหลว');

    expect(service.toasts().map((t) => [t.kind, t.message])).toEqual([
      ['success', 'บันทึกแล้ว'],
      ['error', 'ล้มเหลว'],
    ]);
  });

  it('removes a message by itself: after 4 s, and an error after 7 s', () => {
    service.success('a');
    service.error('b');

    vi.advanceTimersByTime(3999);
    expect(service.toasts().length).toBe(2);

    vi.advanceTimersByTime(1);
    expect(service.toasts().map((t) => t.message)).toEqual(['b']);

    vi.advanceTimersByTime(3000);
    expect(service.toasts()).toEqual([]);
  });

  it('dismiss removes one message and cancels its timer', () => {
    service.info('x');
    const id = service.toasts()[0].id;

    service.dismiss(id);
    service.info('y');
    vi.advanceTimersByTime(3999);

    expect(service.toasts().map((t) => t.message)).toEqual(['y']);
  });

  it('keeps only the five newest', () => {
    for (let i = 1; i <= 7; i++) {
      service.info(`m${i}`);
    }

    expect(service.toasts().map((t) => t.message)).toEqual(['m3', 'm4', 'm5', 'm6', 'm7']);
  });
});

describe('ToastHost', () => {
  let fixture: ComponentFixture<ToastHost>;
  let service: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    fixture = TestBed.createComponent(ToastHost);
    service = TestBed.inject(ToastService);
  });
  afterEach(() => vi.useRealTimers());

  it('shows each message; an error is announced as an alert, the others as a status', () => {
    service.success('บันทึกแล้ว');
    service.error('บันทึกไม่สำเร็จ');
    fixture.detectChanges();

    const items = Array.from(fixture.nativeElement.querySelectorAll('[role]:not(button)')) as HTMLElement[];
    expect(items.map((el) => [el.getAttribute('role'), el.textContent?.replace('✕', '').trim()])).toEqual([
      ['status', 'บันทึกแล้ว'],
      ['alert', 'บันทึกไม่สำเร็จ'],
    ]);
  });

  it('the close button removes the message', () => {
    service.info('ข้อความ');
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('button[aria-label="ปิดข้อความ"]') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(service.toasts()).toEqual([]);
    expect(fixture.nativeElement.textContent).not.toContain('ข้อความ');
  });
});
