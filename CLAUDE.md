# ComSci project rules

Kanchanaburi Rajabhat University Computer Science site: .NET 9 Web API (`backend/`) + Angular 21 (`frontend/`) + SQL Server.

## Working agreements
- Controllers stay thin; logic lives in services. Test the API through Swagger.
- Write verbose, non-abbreviated code. Comments only where needed; none in entities.
- Commit with git after finishing each step.

## Frontend: always use the shared components

Pages under `frontend/src/app/features/` and `layouts/` must be built from the shared components in
`frontend/src/app/shared/`. Do not write raw HTML controls or re-implement them.

| Need | Use | Never |
|---|---|---|
| Single-line input | `app-text-input` | `<input type="text/email/password/number/...">` |
| Multi-line input | `app-text-area` | `<textarea>` |
| Dropdown | `app-select-input` | `<select>` |
| Checkbox / switch | `app-checkbox` / `app-toggle` | `<input type="checkbox">` |
| Date | `app-date-input` | `<input type="date">` |
| File / image upload | `app-file-upload` | `<input type="file">` |
| Label + error text | `app-field` (the inputs already wrap it) | hand-written labels and error `<p>` |
| Button / link button | `button[app-button]`, `a[app-button]` | plain `<button>` with Tailwind classes |
| Table | `app-data-table` with `appCell` templates | `<table>` |
| Paging / search | `app-pagination`, `app-search-box`, state in `PagedList` | own page buttons or debounce code |
| Loading / error / empty | `app-data-state` | own `@if` spinners |
| Confirm | `await ConfirmService.ask({...})` | `window.confirm`, own modal |
| Notice | `ToastService.success/info/error` | `alert()`, own banner |
| Server validation errors | `applyServerErrors`, `describeHttpError` | own parsing of the error body |

Rules:
1. Forms are reactive forms; bind with `formControlName` / `[formControl]`. Error messages come from
   `validation-messages.ts`, not from the page.
2. Import components straight from their file (for example `shared/ui/button/button`), not from the
   `shared/ui/index.ts` barrel, inside `app.ts` and other root-level files (the barrel inflates the initial bundle).
3. If a page needs something the shared components do not do, extend the shared component (add an input, a variant,
   a test) rather than working around it on the page. If a genuinely new control is needed, add it to
   `shared/ui/` with a spec and a section in `frontend/README.md`, then use it.
4. Delete confirmations are mandatory for destructive actions, and the confirm button uses `danger: true`.
5. Only `shared/ui/**` may contain raw `<input>`, `<select>`, `<textarea>`, `<button>`, `<table>`.
   `frontend/src/app/shared/ui/shared-components-rule.spec.ts` fails the test run if a page breaks this.
   A rare exception is allowed with `<!-- shared-ui-exempt: reason -->` on the line above the element.

Run `npm test` (`ng test --watch=false`) in `frontend/` before each commit.
