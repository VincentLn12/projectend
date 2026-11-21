# ComSci project rules

Kanchanaburi Rajabhat University Computer Science site: .NET 9 Web API (`backend/`) + Angular 21 (`frontend/`) + SQL Server.

## Layout and commands

```
backend/src/ComSci.Domain          entities, enums (no dependencies)
backend/src/ComSci.Application     DTOs, service abstractions, options, exceptions, constants
backend/src/ComSci.Infrastructure  EF Core (SQL Server), identity, file storage, service implementations, DependencyInjection.cs
backend/src/ComSci.Api             controllers, middleware, Program.cs
backend/migration/                 one-off import from the old webcomcs site (see its README)
frontend/                          Angular app; /api is proxied to http://localhost:5036 (proxy.conf.json)
```

Dependencies point inward: Api -> Infrastructure -> Application -> Domain.

| Task | Command (folder) |
|---|---|
| Run API, Swagger at `http://localhost:5036/swagger` | `dotnet run` (`backend/src/ComSci.Api`) |
| Apply migrations | `dotnet ef database update --project src/ComSci.Infrastructure --startup-project src/ComSci.Api` (`backend`) |
| Run frontend | `npm start` (`frontend`) |
| Frontend tests / build | `npm test`, `npm run build` (`frontend`) |

Secrets (`Jwt:Key`, `Seed:AdminEmail`, `Seed:AdminPassword`) go in `dotnet user-secrets`, never in git.
Uploads live in `Storage:RootPath` outside the app folder; the database references files by id.
Server settings, rate limits and backup notes are in `backend/README.md`; UI component usage in `frontend/README.md`.
Project docs are written in Thai; keep user-facing text Thai.

## Working agreements
- Controllers stay thin; logic lives in services. Test the API through Swagger.
- Write verbose, non-abbreviated code. Comments only where needed; none in entities.
- Commit with git after finishing each step.

## Backend: structure and conventions

Adding a feature touches these places, in this order:
1. Entity in `Domain/Entities` (inherit `AuditableEntity` for soft delete and audit fields) and its
   `IEntityTypeConfiguration` in `Infrastructure/Persistence/Configurations` (mapping lives there, not in the entity).
2. DTOs in `Application/Dtos/<Feature>` and the service interface in `Application/Abstractions`.
3. Service implementation in `Infrastructure/Services` (registered in `DependencyInjection.cs`).
4. Thin controller in `Api/Controllers`: public endpoints in `<Name>Controller`, admin endpoints in
   `Controllers/Admin/Admin<Name>Controller` with `[Authorize(Roles = Roles.Admin)]`.
5. Add a migration: `dotnet ef migrations add <Name> --project src/ComSci.Infrastructure --startup-project src/ComSci.Api` (in `backend`).

Rules:
1. Controllers only bind input, call one service method and return the result; take a `CancellationToken` on every action.
2. Services signal errors by throwing `NotFoundException`, `BadRequestException` or `ConflictException`
   (`Application/Exceptions`); `ApiExceptionHandler` turns them into ProblemDetails. Do not return error codes by hand.
3. Never expose entities from the API; always map to DTOs. Lists return `PagedResult<T>` and take `PageQuery`.
4. Deletes are soft (`IsDeleted`, handled by the global query filter and `AuditSaveChangesInterceptor`); do not hard-delete.
5. Files go through `IFileService` / `IFileStorage` and `FileRules`; never write to disk or accept uploads directly in a controller.
6. Roles come from `Constants/Roles.cs`; do not hard-code role strings.
7. Validate input with DataAnnotations on the request DTO (`[Required]`, `[MaxLength]`, ...); business rules are checked in the
   service and thrown as `BadRequestException`/`ConflictException`. Messages shown to users are Thai.
8. Endpoints for students or other non-admin users must act only on the caller's own data: take the user from `ICurrentUser`,
   never from an id in the request.
9. Read queries use `AsNoTracking()` and project to DTOs with `Select`; load related data in the same query (no N+1 loops).
10. Never edit a migration that is already committed; add a new one. The app never runs migrations itself.
11. Secrets never go into `appsettings*.json` that is tracked by git (use user-secrets or environment variables).
12. When a DTO changes, update the matching `*.types.ts` and `*.api.ts` in the frontend in the same commit.

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
| Uploaded picture | `app-file-image` (placeholder when missing) | raw `<img src="/api/files/...">` |
| Date shown to visitors | `thaiDate` pipe (Buddhist year) | `date` pipe with English months |
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

## Frontend: Angular style

1. Standalone components only; use `inject()`, signals, and the `@if` / `@for` control flow. New components use
   `ChangeDetectionStrategy.OnPush`.
2. Every page route in `app.routes.ts` is lazy (`loadComponent`); admin routes sit behind `adminGuard`, signed-in routes behind `authGuard`.
3. All text visitors see is Thai. Colors and spacing come from the Tailwind theme tokens, not hex values in templates.
4. Pages must work at phone width (375px) as well as desktop.
5. Never bind API text with `[innerHTML]` unless it goes through Angular's sanitizer; old migrated data may contain HTML.


## Frontend: api and types live inside each feature's own folder

Every feature owns its HTTP calls and its types, so each can be found and changed without touching others:

```
features/admin/news/
  api/news.api.ts          # the only place that calls /api/news (HttpClient)
  types/news.types.ts      # DTOs and request/response types for this feature
  news-list/ news-edit/    # components
core/auth/{api,types}/     # shared infrastructure follows the same shape
core/files/{api,types}/
core/settings/{api,types}/
```

Rules:
1. Components never inject `HttpClient`; they call the feature's `*.api.ts`.
2. Types used by one feature stay in that feature's `types/`. Only types used by several features move to the
   nearest shared folder (`core/<name>/types/` or `shared/data/types/`).
3. A feature does not import another feature's `api/` or `types/`; share through `core/` or `shared/`.
4. File names: `<name>.api.ts`, `<name>.types.ts`.

## Git

- Never commit `bin/`, `obj/`, `dist/`, `node_modules/` or `backend/src/ComSci.Api/uploads/` (already in `.gitignore`).
- One commit per finished step; the message says what changed in plain English (for example "Add job history admin page").
