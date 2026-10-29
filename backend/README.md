# ComSci API (.NET 9)

เว็บ API ของเว็บไซต์สาขาวิทยาการคอมพิวเตอร์ ใช้ SQL Server และเก็บไฟล์อัปโหลดบนดิสก์ของเซิร์ฟเวอร์ (วิธีย้ายข้อมูลจากเว็บเดิมอยู่ใน `migration/README.md`)

## รันบนเครื่องพัฒนา

ครั้งแรก (ในโฟลเดอร์ `backend/src/ComSci.Api`):

```
dotnet user-secrets set "Jwt:Key" "<สุ่มอย่างน้อย 32 ตัวอักษร>"
dotnet user-secrets set "Seed:AdminEmail" "<อีเมลผู้ดูแลคนแรก>"
dotnet user-secrets set "Seed:AdminPassword" "<รหัสผ่านผู้ดูแลคนแรก>"
```

สร้างฐานข้อมูล (ในโฟลเดอร์ `backend`):

```
dotnet ef database update --project src/ComSci.Infrastructure --startup-project src/ComSci.Api
```

แล้วรัน `dotnet run` ในโฟลเดอร์ `src/ComSci.Api` เปิด Swagger ที่ `http://localhost:5036/swagger` (เฉพาะโหมดพัฒนา)

## ตั้งค่าบนเซิร์ฟเวอร์จริง

ตั้งผ่านตัวแปรสภาพแวดล้อม (ใช้ `__` แทน `:`) หรือ `appsettings.Production.json` ห้ามใส่รหัสลับในไฟล์ที่ขึ้น git

| ค่า | ต้องตั้ง | หมายเหตุ |
|---|---|---|
| `ConnectionStrings__Default` | ใช่ | สตริงเชื่อมต่อ SQL Server |
| `Jwt__Key` | ใช่ | อย่างน้อย 32 ตัวอักษร ถ้าเปลี่ยน ทุกคนต้องล็อกอินใหม่ |
| `Storage__RootPath` | ใช่ | **ต้องเป็นพาธเต็มนอกโฟลเดอร์แอป** (เช่น `D:/comsci-uploads`) ระบบไม่ยอมเริ่มถ้าเป็นพาธสัมพัทธ์หรืออยู่ในโฟลเดอร์แอป เพราะ deploy ทับจะทำให้ไฟล์หาย |
| `ForwardedHeaders__KnownProxies__0` | ถ้า proxy อยู่คนละเครื่อง | IP ของ reverse proxy (ค่าเริ่มต้นเชื่อถือเฉพาะเครื่องเดียวกัน) |
| `Cors__Origins__0` | ถ้าเว็บอยู่คนละโดเมนกับ API | ถ้า proxy ส่ง `/api` มาโดเมนเดียวกันไม่ต้องตั้ง |
| `Seed__AdminEmail`, `Seed__AdminPassword` | เฉพาะครั้งแรก | ใช้สร้างผู้ดูแลคนแรก **เมื่อมีผู้ดูแลที่ล็อกอินได้แล้ว ให้ลบสองค่านี้ออก** ไม่งั้นระบบจะสร้างบัญชีนี้กลับมาทุกครั้งที่เริ่ม |

ก่อน deploy: รัน `dotnet ef database update` กับฐานจริง (ระบบไม่รัน migration เอง)

## Reverse proxy

proxy ต้องส่งที่อยู่ของผู้เรียกมาใน `X-Forwarded-For` และ `X-Forwarded-Proto` (nginx: `proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;` และ `proxy_set_header X-Forwarded-Proto $scheme;`) ถ้าไม่ส่ง ทุกคนจะมี IP เดียวกันและถูกจำกัดการล็อกอินรวมกัน

## การจำกัดที่ตั้งไว้

- ล็อกอิน สมัคร รีเฟรช: 10 ครั้งต่อนาทีต่อ IP
- อัปโหลดไฟล์: 20 ไฟล์ต่อชั่วโมงต่อผู้ใช้ (ผู้ดูแลไม่ถูกจำกัด) ไฟล์ละไม่เกิน 10 MB (JPG PNG GIF WebP PDF)
- ล็อกบัญชี 15 นาทีเมื่อใส่รหัสผ่านผิด 5 ครั้ง

## สำรองข้อมูล

ต้องสำรองสองอย่างคู่กัน: ฐานข้อมูล `ComSciDb` และโฟลเดอร์ `Storage__RootPath` ฐานข้อมูลอ้างถึงไฟล์ด้วยรหัส ถ้าอย่างใดอย่างหนึ่งหายจะมีรูปที่แสดงไม่ได้
