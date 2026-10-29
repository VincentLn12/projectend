import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { Button } from '../../shared/ui/button/button';

interface MenuItem {
  label: string;
  path: string;
  exact: boolean;
}

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Button],
  templateUrl: './admin-layout.html',
})
export class AdminLayout {
  protected readonly auth = inject(AuthService);

  // Each admin feature adds one entry here when its screens are built.
  protected readonly menu: MenuItem[] = [
    { label: 'แดชบอร์ด', path: '/admin', exact: true },
    { label: 'ข่าว', path: '/admin/news', exact: false },
    { label: 'โครงงาน', path: '/admin/projects', exact: false },
  ];
}
