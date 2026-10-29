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
