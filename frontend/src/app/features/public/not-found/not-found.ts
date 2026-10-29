import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink],
  template: `
    <section class="py-16 text-center">
      <h1 class="text-2xl font-semibold text-slate-900">ไม่พบหน้าที่ต้องการ</h1>
      <a routerLink="/" class="mt-4 inline-block text-slate-600 underline hover:text-slate-900">กลับหน้าแรก</a>
    </section>
  `,
})
export class NotFound {}
