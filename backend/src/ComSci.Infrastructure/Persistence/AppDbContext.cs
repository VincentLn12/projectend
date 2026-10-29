using System.Linq.Expressions;
using ComSci.Domain.Common;
using ComSci.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ComSci.Infrastructure.Persistence;

/// <summary>
/// ตัวกลางติดต่อฐานข้อมูล SQL Server (ComSciDb)
/// สืบจาก IdentityDbContext จึงมีตารางผู้ใช้/บทบาทของ ASP.NET Identity ให้ด้วย
/// รายละเอียดของแต่ละตาราง (ความยาวคอลัมน์ index ความสัมพันธ์) อยู่ในโฟลเดอร์ Configurations
/// </summary>
public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ---------------- ผู้ใช้และระบบ ----------------
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<StudentProfile> StudentProfiles { get; set; } = null!;
    public DbSet<Lecturer> Lecturers { get; set; } = null!;
    public DbSet<FileRecord> Files { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    // ---------------- เนื้อหาหน้าเว็บ ----------------
    public DbSet<News> News { get; set; } = null!;
    public DbSet<NewsFile> NewsFiles { get; set; } = null!;
    public DbSet<HomeBanner> HomeBanners { get; set; } = null!;
    public DbSet<TechSubject> TechSubjects { get; set; } = null!;
    public DbSet<TechSubjectFile> TechSubjectFiles { get; set; } = null!;
    public DbSet<SystemSetting> SystemSettings { get; set; } = null!;

    // ---------------- โครงงานและศิษย์เก่า ----------------
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectAdvisor> ProjectAdvisors { get; set; } = null!;
    public DbSet<JobHistory> JobHistories { get; set; } = null!;

    // ---------------- หลักสูตรอบรม ----------------
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<CoursePhoto> CoursePhotos { get; set; } = null!;
    public DbSet<Generation> Generations { get; set; } = null!;
    public DbSet<CourseRegistration> CourseRegistrations { get; set; } = null!;
    public DbSet<CourseComment> CourseComments { get; set; } = null!;

    // ---------------- หลักสูตรการศึกษา ----------------
    public DbSet<Syllabus> Syllabuses { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<SyllabusObjective> SyllabusObjectives { get; set; } = null!;
    public DbSet<SyllabusOccupation> SyllabusOccupations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        RenameIdentityTables(builder);

        // โหลดการตั้งค่าของทุกตารางจากคลาส *Configuration ในโฟลเดอร์ Configurations
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ApplySoftDeleteAndDeleteRules(builder);
    }

    /// <summary>
    /// เปลี่ยนชื่อตารางมาตรฐานของ Identity (ปกติขึ้นต้นด้วย AspNet) ให้อ่านง่ายขึ้น
    /// </summary>
    private static void RenameIdentityTables(ModelBuilder builder)
    {
        builder.Entity<AppUser>().ToTable("Users");
        builder.Entity<AppRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
    }

    /// <summary>
    /// กติกาที่ใช้กับทุกตารางพร้อมกัน:
    /// 1) Soft delete: ข้อมูลที่มี IsDeleted = true จะไม่ถูกดึงออกมาใน query ทั่วไป
    /// 2) ข้อมูลลูกจะถูกซ่อนไปด้วยเมื่อข้อมูลแม่ที่บังคับต้องมี (required) ถูกลบ
    /// 3) ไม่ให้ลบแบบ cascade ในฐานข้อมูล (DeleteBehavior.Restrict)
    /// </summary>
    private static void ApplySoftDeleteAndDeleteRules(ModelBuilder builder)
    {
        foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes())
        {
            Type entityClrType = entityType.ClrType;
            ParameterExpression entityParameter = Expression.Parameter(entityClrType, "entity");
            List<Expression> conditions = new List<Expression>();

            // เงื่อนไขที่ 1: แถวนี้เองต้องยังไม่ถูกลบ
            if (typeof(IAuditable).IsAssignableFrom(entityClrType))
            {
                conditions.Add(BuildIsNotDeleted(entityParameter));
            }

            foreach (IMutableForeignKey foreignKey in entityType.GetForeignKeys())
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;

                // เงื่อนไขที่ 2: ถ้าแม่ที่ต้องมีถูกลบ แถวนี้ก็ต้องถูกซ่อน
                IMutableNavigation? navigationToParent = foreignKey.DependentToPrincipal;
                bool parentIsAuditable = typeof(IAuditable).IsAssignableFrom(foreignKey.PrincipalEntityType.ClrType);
                if (foreignKey.IsRequired && navigationToParent != null && parentIsAuditable)
                {
                    Expression parentExpression = Expression.Property(entityParameter, navigationToParent.Name);
                    conditions.Add(BuildIsNotDeleted(parentExpression));
                }
            }

            if (conditions.Count == 0)
            {
                continue;
            }

            // รวมทุกเงื่อนไขด้วย AND แล้วตั้งเป็น query filter ของตารางนี้
            Expression combined = conditions[0];
            for (int index = 1; index < conditions.Count; index++)
            {
                combined = Expression.AndAlso(combined, conditions[index]);
            }

            LambdaExpression filter = Expression.Lambda(combined, entityParameter);
            entityType.SetQueryFilter(filter);
        }
    }

    /// <summary>
    /// สร้างนิพจน์ "target.IsDeleted == false"
    /// </summary>
    private static Expression BuildIsNotDeleted(Expression target)
    {
        Expression isDeleted = Expression.Property(target, nameof(IAuditable.IsDeleted));
        return Expression.Equal(isDeleted, Expression.Constant(false));
    }
}
