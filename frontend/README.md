# Frontend (Angular)

เว็บสาขาวิทยาการคอมพิวเตอร์: Angular 21 (standalone, zoneless) + Tailwind CSS 4

## รัน

```bash
npm install
npm start          # http://localhost:4200
npm test           # unit tests (Vitest)
npm run build
```

`npm start` ส่งทุกคำขอ `/api/...` ต่อไปที่ API (`http://localhost:5036`) ตาม `proxy.conf.json` จึงต้องเปิด API ก่อน (`dotnet run` ในโฟลเดอร์ `backend/src/ComSci.Api`) และไม่ต้องตั้ง CORS ตอนพัฒนา ตอนใช้งานจริงให้เว็บเซิร์ฟเวอร์ที่โฮสต์แอปนี้ส่ง `/api` ไปที่ API เช่นกัน (แอปเรียกด้วย path แบบ relative เสมอ ดู `src/app/core/api.config.ts`)

Node: Angular 21 รองรับ Node 20.19+, 22.12+ และ 24 ตัวที่ทดสอบในเครื่องนี้เป็น Node 26 ซึ่ง CLI เตือนว่ายังไม่รองรับ แต่ build และ test ผ่าน

## โครงสร้าง `src/app`

| โฟลเดอร์ | หน้าที่ |
|---|---|
| `core/` | ของที่ใช้ทั้งแอป: ที่อยู่ API, โมเดลข้อมูล, ระบบล็อกอิน (`auth/`), ข้อมูลตั้งค่าเว็บ (`api/`) |
| `core/auth/` | `AuthService` (เก็บผู้ใช้และ token), `auth.interceptor` (แนบ token และต่ออายุเมื่อได้ 401), `auth.guards` (กันหน้าที่ต้องล็อกอิน/เป็นแอดมิน) |
| `layouts/` | กรอบหน้า: `public-layout` (หัว/ท้ายเว็บ) และ `admin-layout` (เมนูข้าง) |
| `features/public/` | หน้าที่คนทั่วไปเห็น (ตอนนี้: หน้าแรก, เข้าสู่ระบบ, ไม่พบหน้า) |
| `features/admin/` | หน้าผู้ดูแล (ตอนนี้: แดชบอร์ด) |

เพิ่มหน้าใหม่: สร้างโฟลเดอร์ใน `features/...` แล้วเพิ่มเส้นทางแบบ lazy ใน `app.routes.ts` (หน้าผู้ดูแลเพิ่มรายการเมนูใน `admin-layout.ts` ด้วย)

## เรื่องความปลอดภัยของ token

API ส่ง token ใน body (ไม่ใช้ cookie) แอปเก็บ access token (อายุ 15 นาที) ไว้ในหน่วยความจำ และ refresh token (ใช้ได้ครั้งเดียว อายุ 7 วัน) ไว้ใน `localStorage` เพื่อให้โหลดหน้าใหม่แล้วยังล็อกอินอยู่ ข้อเสียคือสคริปต์ที่แทรกเข้ามาในหน้า (XSS) อ่าน refresh token ได้ จึงห้ามใส่ HTML ที่ไม่ผ่านการตรวจสอบลงในหน้า (Angular กรองให้โดยปกติ อย่าใช้ `innerHTML`/`bypassSecurityTrust*` กับข้อมูลจากผู้ใช้) และการเข้าถึงหน้าผู้ดูแลที่ฝั่งแอปเป็นแค่การซ่อนหน้า สิทธิ์จริงตรวจที่ API ทุกครั้ง

## ชุด component ฟอร์ม (`src/app/shared/ui`)

ใช้ได้กับ Reactive Forms (`formControlName` หรือ `[formControl]`) โดยตรง ข้อความข้อผิดพลาดภาษาไทยและดาวแดงของช่องที่จำเป็น (`Validators.required`) ขึ้นให้เอง ข้อผิดพลาดจะแสดงหลังผู้ใช้แตะหรือแก้ช่อง หรือเมื่อสั่ง `form.markAllAsTouched()`

```html
<form [formGroup]="form" (ngSubmit)="save()">
  <app-text-input label="หัวข้อ" formControlName="title" hint="ไม่เกิน 300 ตัวอักษร" />
  <app-text-input label="ปี (พ.ศ.)" type="number" formControlName="year" />
  <app-text-area label="เนื้อหา" formControlName="body" [maxLength]="1000" />
  <app-select-input label="ประเภท" formControlName="kind" [options]="kinds" />
  <app-date-input label="วันที่" formControlName="date" />
  <app-toggle label="เผยแพร่" formControlName="isPublished" />
  <app-file-upload label="รูปประกอบ" formControlName="fileIds" [multiple]="true" />
</form>
```

| Component | ค่าในฟอร์ม |
|---|---|
| `app-text-input` (`type`: text, email, password, search, tel, url, number) | `string` (`number` เมื่อ `type="number"`, ว่าง = `null`) |
| `app-text-area` | `string` |
| `app-select-input` (`[options]` = `{ value, label }[]`) | ค่าจริงของตัวเลือก (ตัวเลข, enum, id ได้) หรือ `null` |
| `app-checkbox`, `app-toggle` | `boolean` |
| `app-date-input` (`type`: date, datetime-local) | ข้อความจาก browser (`2030-01-31` / `2030-01-31T09:30`) หรือ `null` |
| `app-file-upload` (`accept`: image, pdf, any; `[multiple]`) | รหัสไฟล์ (`string \| null`) หรือรายการรหัส (`string[]`) เมื่อ multiple โดยรหัสแรกคือรูปหลัก |

ตัวช่วยใน `src/app/shared/forms`:

- `applyServerErrors(form, error)` นำข้อความ 400 จาก API ไปวางใต้ช่องที่ตรงชื่อ (`Title` → `title`) และคืนข้อความสำหรับปัญหาที่ไม่เกี่ยวกับช่องใดช่องหนึ่ง
- `urlValidator`, `notEmptyListValidator`
- `localDateTimeToIso` / `isoToLocalDateTime` แปลงค่าของ `datetime-local` (ไม่มีเขตเวลา) กับเวลา UTC ที่ API ใช้ ต้องแปลงก่อนส่งทุกครั้ง
- `FormControlBase` คลาสแม่สำหรับสร้าง component ฟอร์มใหม่ (ดูตัวอย่างจาก `text-input`)

ดูและลองใช้ทั้งชุดได้ที่ `/dev/components` (เปิดเฉพาะตอน `npm start` ใน build จริงไม่มีเส้นทางนี้ แต่ไฟล์ของหน้าตัวอย่างราว 8 kB ยังถูกรวมอยู่ในผลลัพธ์ โดยไม่ถูกโหลดถ้าไม่มีใครเข้าหน้านั้น) ช่องอัปโหลดต้องล็อกอินก่อน เพราะ `POST /api/files` ต้องมี token

## Component แสดงผลและโต้ตอบ (`src/app/shared/ui`, `src/app/shared/data`)

```html
<!-- ปุ่ม: ใส่แอตทริบิวต์ app-button ให้ <button> หรือ <a> จริง จึงใช้ type, disabled, routerLink ได้ตามปกติ -->
<button type="submit" app-button [loading]="saving()">บันทึก</button>
<button type="button" app-button variant="danger" size="sm" (click)="remove(row)">ลบ</button>

<!-- ค้นหา + ตาราง + แบ่งหน้า (สถานะโหลด/ว่าง/ผิดพลาดอยู่ในตารางเอง) -->
<app-search-box placeholder="ค้นหาข่าว" (searchChange)="list.setSearch($event)" />
<app-data-table [columns]="columns" [rows]="list.items()" [loading]="list.loading()" [error]="list.error()"
                emptyMessage="ยังไม่มีข่าว" (retry)="list.reload()">
  <ng-template appCell="actions" let-row> <button app-button size="sm" variant="ghost" (click)="remove(row)">ลบ</button> </ng-template>
</app-data-table>
<app-pagination [page]="list.page()" [pageSize]="list.pageSize" [total]="list.total()" (pageChange)="list.setPage($event)" />
```

```ts
// ในคลาส component ของหน้า
protected readonly list = createPagedList((q) => this.api.getNews(q), 10);  // q = { page, pageSize, search }
private readonly confirm = inject(ConfirmService);
private readonly toast = inject(ToastService);

constructor() { this.list.load(); }

async remove(row: News) {
  if (!(await this.confirm.ask({ title: `ลบ "${row.title}" ?`, danger: true, confirmText: 'ลบ' }))) return;
  this.api.delete(row.id).subscribe({
    next: () => { this.toast.success('ลบแล้ว'); this.list.reload(); },
    error: (e) => this.toast.error(describeHttpError(e)),
  });
}
```

| ชิ้นส่วน | หน้าที่ |
|---|---|
| `button[app-button]` | `variant`: primary, secondary, danger, ghost / `size`: sm, md / `loading` แสดงวงกลมหมุนและกดซ้ำไม่ได้ / `disabled` |
| `app-data-state` | ห่อเนื้อหาที่รอข้อมูล: กำลังโหลด, ผิดพลาด (มีปุ่มลองใหม่), ว่าง, หรือแสดงเนื้อหา |
| `app-data-table` | `columns` (`key`, `header`, `value`, `align`) และ `rows` ช่องที่ต้องการวาดเองใช้ `<ng-template appCell="key" let-row>` |
| `app-pagination` | หน้าเริ่มที่ 1 เหมือน API แสดงช่วงแถวและเลขหน้า พร้อม … เมื่อมีหลายหน้า |
| `app-search-box` | รอให้หยุดพิมพ์ 0.3 วินาทีก่อนส่งคำค้น (Enter ส่งทันที) มีปุ่มล้าง |
| `PagedList` / `createPagedList` | สถานะของหน้ารายการ: แถว หน้า คำค้น กำลังโหลด ข้อผิดพลาด คำขอล่าสุดเท่านั้นที่มีผล และถ้าหน้าปัจจุบันว่างหลังลบแถวสุดท้ายจะถอยไปหน้าก่อนหน้าเอง |
| `ConfirmService.ask(...)` | กล่องยืนยันแบบ `<dialog>` ของ browser (Esc, คลิกพื้นหลัง = ไม่ยืนยัน) โฟกัสเริ่มที่ปุ่มยกเลิก คืนค่า `Promise<boolean>` |
| `ToastService.success/info/error(...)` | ข้อความมุมจอ หายเองใน 4 วินาที (ข้อผิดพลาด 7 วินาที) หรือกดปิด แสดงสูงสุด 5 ข้อความ |
| `describeHttpError(error)` | ข้อความไทยสั้นๆ สำหรับคำขอที่ล้มเหลวนอกฟอร์ม |

`ConfirmDialog` และ `ToastHost` วางไว้ครั้งเดียวใน `app.ts` หน้าไหนก็เรียกบริการได้เลย ดูทุกชิ้นทำงานได้ที่ `/dev/components/display`

ข้อควรระวังขนาดไฟล์: ในไฟล์รากของแอป (`app.ts`, `app.config.ts`) ให้ import ตรงจากไฟล์ของ component (เช่น `./shared/ui/toast/toast-host`) ไม่ผ่าน `shared/ui/index.ts` เพราะไฟล์รวมลากทุก component เข้าไฟล์เริ่มต้น (วัดแล้วเพิ่มราว 60 kB) ส่วนหน้าที่โหลดแบบ lazy import ผ่านไฟล์รวมได้
