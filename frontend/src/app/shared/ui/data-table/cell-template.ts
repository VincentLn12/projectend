import { Directive, TemplateRef, inject, input } from '@angular/core';

// Lets a page draw its own content in one column of a data table (buttons, badges, links):
//   <ng-template appCell="actions" let-row> <button ...>แก้ไข</button> </ng-template>
@Directive({ selector: 'ng-template[appCell]' })
export class CellTemplate {
  readonly key = input.required<string>({ alias: 'appCell' });
  readonly template = inject<TemplateRef<unknown>>(TemplateRef);
}
