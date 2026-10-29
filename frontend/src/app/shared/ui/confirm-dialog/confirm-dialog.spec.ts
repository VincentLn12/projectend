import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfirmDialog } from './confirm-dialog';
import { ConfirmService } from './confirm.service';

// The test environment has no real <dialog>: give it the two methods the component calls.
function installDialogStub(): void {
  const prototype = ((globalThis as { HTMLDialogElement?: { prototype: object } }).HTMLDialogElement?.prototype ??
    HTMLElement.prototype) as { showModal?: () => void; close?: () => void };

  prototype.showModal = function (this: HTMLElement) {
    this.setAttribute('open', '');
  };
  prototype.close = function (this: HTMLElement) {
    this.removeAttribute('open');
  };
}

describe('ConfirmDialog', () => {
  let fixture: ComponentFixture<ConfirmDialog>;
  let service: ConfirmService;

  const dialog = () => fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
  const button = (text: string) =>
    (Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[]).find((b) => b.textContent?.trim() === text)!;

  async function settle(): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  }

  beforeEach(async () => {
    installDialogStub();
    fixture = TestBed.createComponent(ConfirmDialog);
    service = TestBed.inject(ConfirmService);
    await settle();
  });

  it('stays closed until someone asks', () => {
    expect(dialog().hasAttribute('open')).toBe(false);
  });

  it('opens with the title and message, and "confirm" answers yes', async () => {
    const answer = service.ask({ title: 'ลบข่าวนี้?', message: 'ลบแล้วกู้คืนไม่ได้', confirmText: 'ลบ' });
    await settle();

    expect(dialog().hasAttribute('open')).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('ลบข่าวนี้?');
    expect(fixture.nativeElement.textContent).toContain('ลบแล้วกู้คืนไม่ได้');

    button('ลบ').click();
    await settle();

    expect(await answer).toBe(true);
    expect(dialog().hasAttribute('open')).toBe(false);
  });

  it('puts the keyboard focus on "cancel" when it opens, so Enter does not delete by accident', async () => {
    service.ask({ title: 'ลบ?', danger: true });
    await settle();

    expect(document.activeElement).toBe(button('ยกเลิก'));
  });

  it('"cancel" answers no and closes', async () => {
    const answer = service.ask({ title: 'ลบ?' });
    await settle();

    button('ยกเลิก').click();
    await settle();

    expect(await answer).toBe(false);
    expect(dialog().hasAttribute('open')).toBe(false);
  });

  it('Esc answers no (the browser cancel event)', async () => {
    const answer = service.ask({ title: 'ลบ?' });
    await settle();

    const escape = new Event('cancel', { cancelable: true });
    dialog().dispatchEvent(escape);
    await settle();

    expect(escape.defaultPrevented).toBe(true); // the dialog is closed by our state, not by the browser
    expect(await answer).toBe(false);
  });

  it('a click on the dimmed area answers no, a click inside the box does not', async () => {
    const answer = service.ask({ title: 'ลบ?' });
    await settle();

    fixture.nativeElement.querySelector('h2').dispatchEvent(new MouseEvent('click', { bubbles: true }));
    await settle();
    expect(dialog().hasAttribute('open')).toBe(true);

    dialog().dispatchEvent(new MouseEvent('click'));
    await settle();
    expect(await answer).toBe(false);
  });

  it('a dangerous question gets a red confirm button; otherwise it is the normal one', async () => {
    service.ask({ title: 'ลบ?', danger: true });
    await settle();
    expect(button('ยืนยัน').className).toContain('bg-red-600');

    service.answer(false);
    service.ask({ title: 'บันทึก?' });
    await settle();
    expect(button('ยืนยัน').className).toContain('bg-slate-900');
  });

  it('asking a second question while one is open answers the first with no', async () => {
    const first = service.ask({ title: 'คำถามแรก' });
    await settle();

    const second = service.ask({ title: 'คำถามที่สอง' });
    await settle();

    expect(await first).toBe(false);
    expect(fixture.nativeElement.textContent).toContain('คำถามที่สอง');

    button('ยืนยัน').click();
    expect(await second).toBe(true);
  });
});
