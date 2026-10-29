import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { createPagedList } from '../../../../shared/data/paged-list';
import { Button } from '../../../../shared/ui/button/button';
import { ConfirmService } from '../../../../shared/ui/confirm-dialog/confirm.service';
import { CellTemplate } from '../../../../shared/ui/data-table/cell-template';
import { DataTable, TableColumn } from '../../../../shared/ui/data-table/data-table';
import { Pagination } from '../../../../shared/ui/pagination/pagination';
import { SearchBox } from '../../../../shared/ui/search-box/search-box';
import { SelectInput, SelectOption } from '../../../../shared/ui/select-input/select-input';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { AdminUsersApi, describeUserError } from '../api/users.api';
import { AdminUserListItem, USER_TYPE_LABELS } from '../types/user.types';

@Component({
  selector: 'app-user-list',
  imports: [RouterLink, ReactiveFormsModule, Button, DataTable, CellTemplate, Pagination, SearchBox, SelectInput],
  templateUrl: './user-list.html',
})
export class UserList implements OnInit {
  private readonly api = inject(AdminUsersApi);
  private readonly confirm = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected readonly userType = new FormControl<number | null>(null);
  protected readonly typeOptions: SelectOption<number | null>[] = [
    { value: null, label: 'ทุกประเภท' },
    ...Object.entries(USER_TYPE_LABELS).map(([value, label]) => ({ value: Number(value), label })),
  ];

  protected readonly list = createPagedList<AdminUserListItem>((query) => this.api.list(query, this.userType.value));

  protected readonly columns: TableColumn<AdminUserListItem>[] = [
    { key: 'fullName', header: 'ชื่อ-สกุล' },
    { key: 'email', header: 'อีเมล', value: (row) => row.email },
    { key: 'userType', header: 'ประเภท', value: (row) => USER_TYPE_LABELS[row.userType] },
    { key: 'studentCode', header: 'รหัสนักศึกษา', value: (row) => row.studentCode },
    { key: 'status', header: 'สถานะ' },
    { key: 'actions', header: '', align: 'right' },
  ];

  protected readonly rowKey = (row: AdminUserListItem) => row.id;

  constructor() {
    this.userType.valueChanges.pipe(takeUntilDestroyed(inject(DestroyRef))).subscribe(() => this.list.setPage(1));
  }

  ngOnInit(): void {
    this.list.load();
  }

  protected async toggleLocked(row: AdminUserListItem): Promise<void> {
    const lock = !row.isLocked;
    if (lock) {
      const confirmed = await this.confirm.ask({
        title: 'ล็อกบัญชีนี้?',
        message: `"${row.fullName}" จะเข้าสู่ระบบไม่ได้จนกว่าจะปลดล็อก`,
        confirmText: 'ล็อกบัญชี',
        danger: true,
      });
      if (!confirmed) {
        return;
      }
    }

    this.api.setLocked(row.id, lock).subscribe({
      next: () => {
        this.toast.success(lock ? 'ล็อกบัญชีแล้ว' : 'ปลดล็อกบัญชีแล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeUserError(error)),
    });
  }

  protected async askToDelete(row: AdminUserListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'ลบผู้ใช้นี้?',
      message: `"${row.fullName}" (${row.email}) จะถูกลบออกจากระบบ`,
      confirmText: 'ลบ',
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('ลบผู้ใช้แล้ว');
        this.list.reload();
      },
      error: (error: unknown) => this.toast.error(describeUserError(error)),
    });
  }
}
