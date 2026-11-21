import { Location } from '@angular/common';
import { Directive, inject, input } from '@angular/core';
import { Router } from '@angular/router';

// A "back" link that returns to the page the visitor came from (for example from the student list to a user and
// back to the student list). When this page was opened directly (a bookmark, a new tab) there is no page to return
// to, so it goes to the fallback list instead. Keep the href for the fallback so the link still works with the
// context menu and "open in a new tab".
//   <a app-button appBack="/admin/news" href="/admin/news">กลับไปรายการข่าว</a>
@Directive({
  selector: 'a[appBack]',
  host: { '(click)': 'goBack($event)' },
})
export class BackLink {
  readonly appBack = input.required<string>();

  private readonly location = inject(Location);
  private readonly router = inject(Router);

  protected goBack(event: MouseEvent): void {
    if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) {
      return;
    }
    event.preventDefault();

    // The router numbers every navigation inside the app from 1; a number above 1 means an earlier page exists.
    const navigationId: number = history.state?.navigationId ?? 0;
    if (navigationId > 1) {
      this.location.back();
    } else {
      this.router.navigateByUrl(this.appBack());
    }
  }
}
