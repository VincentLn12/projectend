import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';

export interface TabItem {
  key: string;
  label: string;
  count?: number | null;
}

// A row of tabs for switching between sections of one page. The page decides what to show for the chosen key:
//   <app-tabs [tabs]="tabs" [(value)]="activeTab" />
//   @if (activeTab() === 'projects') { ... } @else { ... }
// Left/right arrow keys move between the tabs.
@Component({
  selector: 'app-tabs',
  templateUrl: './tabs.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block' },
})
export class Tabs {
  readonly tabs = input.required<TabItem[]>();
  readonly value = model.required<string>();
  readonly label = input('เลือกส่วนที่ต้องการดู');

  protected choose(key: string): void {
    this.value.set(key);
  }

  protected moveBy(step: number, event: Event): void {
    event.preventDefault();
    const tabs = this.tabs();
    const currentIndex = tabs.findIndex((tab) => tab.key === this.value());
    const next = tabs[(currentIndex + step + tabs.length) % tabs.length];
    this.value.set(next.key);
    const buttons = (event.currentTarget as HTMLElement).parentElement?.querySelectorAll<HTMLElement>('[role="tab"]');
    buttons?.[tabs.indexOf(next)]?.focus();
  }
}
