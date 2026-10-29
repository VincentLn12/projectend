using System.Linq.Expressions;
using ComSci.Domain.Common;
using ComSci.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, AppRole, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<Lecturer> Lecturers => Set<Lecturer>();
    public DbSet<FileRecord> Files => Set<FileRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<News> News => Set<News>();
    public DbSet<NewsFile> NewsFiles => Set<NewsFile>();
    public DbSet<HomeBanner> HomeBanners => Set<HomeBanner>();
    public DbSet<TechSubject> TechSubjects => Set<TechSubject>();
    public DbSet<TechSubjectFile> TechSubjectFiles => Set<TechSubjectFile>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectAdvisor> ProjectAdvisors => Set<ProjectAdvisor>();
    public DbSet<JobHistory> JobHistories => Set<JobHistory>();

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CoursePhoto> CoursePhotos => Set<CoursePhoto>();
    public DbSet<Generation> Generations => Set<Generation>();
    public DbSet<CourseRegistration> CourseRegistrations => Set<CourseRegistration>();
    public DbSet<CourseComment> CourseComments => Set<CourseComment>();

    public DbSet<Syllabus> Syllabuses => Set<Syllabus>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<SyllabusObjective> SyllabusObjectives => Set<SyllabusObjective>();
    public DbSet<SyllabusOccupation> SyllabusOccupations => Set<SyllabusOccupation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>().ToTable("Users");
        builder.Entity<AppRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clr = entityType.ClrType;
            var e = Expression.Parameter(clr, "e");
            var conditions = new List<Expression>();

            // Soft delete: hide rows flagged IsDeleted from every query.
            if (typeof(IAuditable).IsAssignableFrom(clr))
                conditions.Add(NotDeleted(e));

            foreach (var fk in entityType.GetForeignKeys())
            {
                // Hard deletes never cascade; rows are soft-deleted instead.
                fk.DeleteBehavior = DeleteBehavior.Restrict;

                // A child is hidden together with a soft-deleted required parent.
                var nav = fk.DependentToPrincipal;
                if (fk.IsRequired && nav is not null && typeof(IAuditable).IsAssignableFrom(fk.PrincipalEntityType.ClrType))
                    conditions.Add(NotDeleted(Expression.Property(e, nav.Name)));
            }

            if (conditions.Count > 0)
                entityType.SetQueryFilter(Expression.Lambda(conditions.Aggregate(Expression.AndAlso), e));
        }
    }

    private static Expression NotDeleted(Expression target) =>
        Expression.Equal(Expression.Property(target, nameof(IAuditable.IsDeleted)), Expression.Constant(false));
}
