import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { ConfirmService } from '../ui/confirm-dialog/confirm.service';

export interface HasUnsavedChanges {
  hasUnsavedChanges(): boolean;
}

// Put on a route whose component implements HasUnsavedChanges: leaving with edits that were not saved asks first.
export const unsavedChangesGuard: CanDeactivateFn<HasUnsavedChanges> = (component) => {
  if (!component.hasUnsavedChanges()) {
    return true;
  }
  return inject(ConfirmService).ask({
    title: 'ออกจากหน้านี้?',
    message: 'มีการแก้ไขที่ยังไม่ได้บันทึก หากออกจากหน้านี้ข้อมูลที่แก้ไขจะหายไป',
    confirmText: 'ออกจากหน้านี้',
    danger: true,
  });
};
