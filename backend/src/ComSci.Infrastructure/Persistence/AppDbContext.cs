using System.Linq.Expressions;
using ComSci.Domain.Common;
using ComSci.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ComSci.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<StudentProfile> StudentProfiles { get; set; } = null!;
    public DbSet<Lecturer> Lecturers { get; set; } = null!;
    public DbSet<FileRecord> Files { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    public DbSet<News> News { get; set; } = null!;
    public DbSet<NewsFile> NewsFiles { get; set; } = null!;
    public DbSet<HomeBanner> HomeBanners { get; set; } = null!;
    public DbSet<TechSubject> TechSubjects { get; set; } = null!;
    public DbSet<TechSubjectFile> TechSubjectFiles { get; set; } = null!;
    public DbSet<SystemSetting> SystemSettings { get; set; } = null!;

    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectAdvisor> ProjectAdvisors { get; set; } = null!;
    public DbSet<JobHistory> JobHistories { get; set; } = null!;

    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<CoursePhoto> CoursePhotos { get; set; } = null!;
    public DbSet<Generation> Generations { get; set; } = null!;
    public DbSet<CourseRegistration> CourseRegistrations { get; set; } = null!;
    public DbSet<CourseComment> CourseComments { get; set; } = null!;

    public DbSet<ShortCourse> ShortCourses { get; set; } = null!;
    public DbSet<ShortCourseRegistration> ShortCourseRegistrations { get; set; } = null!;
    public DbSet<CertificateTemplate> CertificateTemplates { get; set; } = null!;

    public DbSet<Syllabus> Syllabuses { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<SyllabusObjective> SyllabusObjectives { get; set; } = null!;
    public DbSet<SyllabusOccupation> SyllabusOccupations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        RenameIdentityTables(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ApplySoftDeleteAndDeleteRules(builder);
    }

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

    // กติกาทุกตาราง: ซ่อนแถว IsDeleted ใน query, ซ่อนแถวลูกเมื่อแม่ที่บังคับต้องมีถูกลบ, ไม่ cascade delete
    private static void ApplySoftDeleteAndDeleteRules(ModelBuilder builder)
    {
        foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes())
        {
            Type entityClrType = entityType.ClrType;
            ParameterExpression entityParameter = Expression.Parameter(entityClrType, "entity");
            List<Expression> conditions = new List<Expression>();

            if (typeof(IAuditable).IsAssignableFrom(entityClrType))
            {
                conditions.Add(BuildIsNotDeleted(entityParameter));
            }

            foreach (IMutableForeignKey foreignKey in entityType.GetForeignKeys())
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;

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

            Expression combined = conditions[0];
            for (int index = 1; index < conditions.Count; index++)
            {
                combined = Expression.AndAlso(combined, conditions[index]);
            }

            LambdaExpression filter = Expression.Lambda(combined, entityParameter);
            entityType.SetQueryFilter(filter);
        }
    }

    private static Expression BuildIsNotDeleted(Expression target)
    {
        Expression isDeleted = Expression.Property(target, nameof(IAuditable.IsDeleted));
        return Expression.Equal(isDeleted, Expression.Constant(false));
    }
}
