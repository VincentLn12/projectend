using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class ShortCourseRegistrationConfiguration : IEntityTypeConfiguration<ShortCourseRegistration>
{
    public void Configure(EntityTypeBuilder<ShortCourseRegistration> builder)
    {
        builder.Property(registration => registration.Prefix)
            .HasMaxLength(50);

        builder.Property(registration => registration.FirstName)
            .HasMaxLength(100);

        builder.Property(registration => registration.LastName)
            .HasMaxLength(100);

        builder.Property(registration => registration.Phone)
            .HasMaxLength(20);

        builder.Property(registration => registration.Status)
            .HasConversion<byte>();

        builder.HasOne(registration => registration.ShortCourse)
            .WithMany(shortCourse => shortCourse.Registrations)
            .HasForeignKey(registration => registration.ShortCourseId);

        // 3 = RegistrationStatus.Cancelled: เบอร์เดียวสมัครซ้ำไม่ได้ แต่ยกเลิกแล้วสมัครใหม่ได้
        builder.HasIndex(registration => new { registration.ShortCourseId, registration.Phone })
            .IsUnique()
            .HasFilter("[Status] <> 3 AND [IsDeleted] = 0");
    }
}
