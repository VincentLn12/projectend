import { ChangeDetectionStrategy, Component, DestroyRef, inject, input, linkedSignal, output } from '@angular/core';
import { inputClass } from '../styles';

// A search field for list pages. It waits until the person stops typing (300 ms by default) before it
// announces the text, so the list is not reloaded on every key. Enter announces it at once.
@Component({
  selector: 'app-search-box',
  templateUrl: './search-box.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchBox {
  readonly placeholder = input('ค้นหา…');
  readonly value = input('');
  readonly debounceMs = input(300);
  readonly searchChange = output<string>();

  protected readonly inputClass = inputClass;
  protected readonly text = linkedSignal(() => this.value());

  private timer: ReturnType<typeof setTimeout> | null = null;
  private lastAnnounced: string | null = null;

  constructor() {
    inject(DestroyRef).onDestroy(() => this.cancelTimer());
  }

  protected onInput(event: Event): void {
    this.text.set((event.target as HTMLInputElement).value);
    this.cancelTimer();
    this.timer = setTimeout(() => this.announce(), this.debounceMs());
  }

  protected submitNow(): void {
    this.cancelTimer();
    this.announce();
  }

  protected clear(): void {
    this.text.set('');
    this.submitNow();
  }

  private announce(): void {
    const trimmed = this.text().trim();
    if (trimmed === (this.lastAnnounced ?? this.value().trim())) {
      return;
    }
    this.lastAnnounced = trimmed;
    this.searchChange.emit(trimmed);
  }

  private cancelTimer(): void {
    if (this.timer !== null) {
      clearTimeout(this.timer);
      this.timer = null;
    }
  }
}
