using ComSci.Application.Abstractions;
using ComSci.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ComSci.Infrastructure.Persistence;

/// <summary>
/// ทำงานทุกครั้งก่อนบันทึกลงฐานข้อมูล:
/// 1) เติม CreatedAt/CreatedBy ตอนเพิ่มข้อมูล และ UpdatedAt/UpdatedBy ตอนแก้ไข
/// 2) เปลี่ยนคำสั่ง "ลบ" ของข้อมูลที่ติดตามประวัติ (IAuditable) ให้เป็น soft delete (IsDeleted = true)
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    public AuditSaveChangesInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAuditRules(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditRules(DbContext? dbContext)
    {
        if (dbContext == null)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        Guid? userId = _currentUser.UserId;

        foreach (EntityEntry<IAuditable> entry in dbContext.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
            else if (entry.State == EntityState.Deleted)
            {
                // ไม่ลบจริง: เปลี่ยนเป็นการแก้ไขแถว โดยตั้ง IsDeleted = true
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
        }
    }
}
