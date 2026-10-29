import { ChangeDetectionStrategy, Component, ElementRef, computed, inject, input, signal, viewChild } from '@angular/core';
import { FormControlBase } from '../../forms/form-control-base';
import { Field } from '../field/field';
import { inputClass, softInputClass } from '../styles';

export interface SelectOption<T> {
  value: T;
  label: string;
}

interface ListedRow {
  index: number;
  label: string;
  selected: boolean;
}

const AUTO_SEARCH_MIN_OPTIONS = 8;
const LIST_WIDTH_PX = 320;
const SCREEN_EDGE_MARGIN_PX = 16;
const NO_OPTION_INDEX = -1;

// A drop-down list. The value can be any type (number, enum, id): the list position is what the browser sees,
// and it is mapped back to the real value here.
// A long list gets a search box: with [searchable] left out, a list of 8 or more options is searchable (the browser's own
// <select> cannot search); [searchable]="true" or "false" forces it either way. The search ignores upper/lower case.
@Component({
  selector: 'app-select-input',
  imports: [Field],
  templateUrl: './select-input.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:click)': 'closeWhenClickedOutside($event)',
    '(keydown.escape)': 'close()',
  },
})
export class SelectInput<T> extends FormControlBase<T> {
  readonly options = input.required<SelectOption<T>[]>();
  readonly placeholder = input('เลือก…');
  readonly shape = input<'default' | 'soft'>('default');
  readonly searchable = input<boolean | null>(null);
  readonly searchPlaceholder = input('พิมพ์เพื่อค้นหา…');
  readonly noResultsText = input('ไม่พบตัวเลือกที่ตรงกัน');

  protected readonly controlClass = computed(() =>
    this.shape() === 'soft' ? softInputClass(!!this.errorMessage()) : inputClass(!!this.errorMessage()),
  );
  protected readonly selectedIndex = computed(() => this.options().findIndex((option) => option.value === this.value()));
  protected readonly isSearchable = computed(() => this.searchable() ?? this.options().length >= AUTO_SEARCH_MIN_OPTIONS);

  protected readonly isOpen = signal(false);
  protected readonly opensTowardsStart = signal(false);
  protected readonly searchText = signal('');
  protected readonly activePosition = signal(0);

  protected readonly selectedLabel = computed(() => {
    const index = this.selectedIndex();
    return index < 0 ? null : this.options()[index].label;
  });

  protected readonly rows = computed<ListedRow[]>(() => {
    const searchText = this.searchText().trim().toLowerCase();
    const selectedIndex = this.selectedIndex();
    const rows: ListedRow[] = [];

    if (searchText === '') {
      rows.push({ index: NO_OPTION_INDEX, label: this.placeholder(), selected: selectedIndex < 0 });
    }
    this.options().forEach((option, index) => {
      if (searchText === '' || option.label.toLowerCase().includes(searchText)) {
        rows.push({ index, label: option.label, selected: index === selectedIndex });
      }
    });
    return rows;
  });

  protected readonly hasNoMatches = computed(() => this.searchText().trim() !== '' && this.rows().length === 0);

  private readonly hostElement = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly searchBox = viewChild<ElementRef<HTMLInputElement>>('searchBox');

  protected onSelect(event: Event): void {
    const index = Number((event.target as HTMLSelectElement).value);
    this.update(index < 0 ? null : this.options()[index].value);
  }

  protected toggleList(): void {
    if (this.isOpen()) {
      this.close();
      return;
    }
    this.openList();
  }

  protected openFromKeyboard(event: Event): void {
    event.preventDefault();
    if (!this.isOpen()) {
      this.openList();
    }
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

  protected chooseRow(row: ListedRow): void {
    this.update(row.index < 0 ? null : this.options()[row.index].value);
    this.close();
  }

  protected onSearchInput(event: Event): void {
    this.searchText.set((event.target as HTMLInputElement).value);
    this.activePosition.set(0);
  }

  protected onSearchKeydown(event: KeyboardEvent): void {
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.moveActive(1);
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.moveActive(-1);
    } else if (event.key === 'Enter') {
      event.preventDefault();
      const row = this.rows()[this.activePosition()];
      if (row) {
        this.chooseRow(row);
      }
    }
  }

  protected setActivePosition(position: number): void {
    this.activePosition.set(position);
  }

  private openList(): void {
    this.searchText.set('');
    const selectedPosition = this.rows().findIndex((row) => row.selected);
    this.activePosition.set(Math.max(selectedPosition, 0));

    const fieldLeft = this.hostElement.nativeElement.getBoundingClientRect().left;
    this.opensTowardsStart.set(fieldLeft + LIST_WIDTH_PX > window.innerWidth - SCREEN_EDGE_MARGIN_PX);

    this.isOpen.set(true);
    setTimeout(() => {
      this.searchBox()?.nativeElement.focus();
      this.scrollActiveRowIntoView();
    });
  }

  private moveActive(step: number): void {
    const count = this.rows().length;
    if (count === 0) {
      return;
    }
    this.activePosition.set((this.activePosition() + step + count) % count);
    setTimeout(() => this.scrollActiveRowIntoView());
  }

  private scrollActiveRowIntoView(): void {
    const row = this.hostElement.nativeElement.querySelector(`[data-position="${this.activePosition()}"]`);
    row?.scrollIntoView({ block: 'nearest' });
  }
}
