import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// The title block at the top of an admin list page: a title, an optional count and description, and the page's
// main action (put the "add" button inside the tag).
//   <app-page-header title="จัดการข่าว" [count]="list.total()" unit="รายการ">
//     <a app-button routerLink="/admin/news/new">เพิ่มข่าว</a>
//   </app-page-header>
@Component({
  selector: 'app-page-header',
  templateUrl: './page-header.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block' },
})
export class PageHeader {
  readonly title = input.required<string>();
  readonly description = input('');
  readonly count = input<number | null>(null);
  readonly unit = input('รายการ');
}
