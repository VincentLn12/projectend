using ComSci.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComSci.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> b)
    {
        b.Property(x => x.Title).HasMaxLength(300);
        b.HasOne(x => x.Lecturer).WithMany().HasForeignKey(x => x.LecturerId);
    }
}

public class CoursePhotoConfiguration : IEntityTypeConfiguration<CoursePhoto>
{
    public void Configure(EntityTypeBuilder<CoursePhoto> b)
    {
        b.HasOne(x => x.Course).WithMany(c => c.Photos).HasForeignKey(x => x.CourseId);
        b.HasOne(x => x.File).WithMany().HasForeignKey(x => x.FileId);
    }
}

public class GenerationConfiguration : IEntityTypeConfiguration<Generation>
{
    public void Configure(EntityTypeBuilder<Generation> b)
    {
        b.Property(x => x.SubTitle).HasMaxLength(300);
        b.Property(x => x.Fee).HasPrecision(10, 2);
        b.HasOne(x => x.Course).WithMany(c => c.Generations).HasForeignKey(x => x.CourseId);
        b.HasOne(x => x.PhotoFile).WithMany().HasForeignKey(x => x.PhotoFileId);
    }
}

public class CourseRegistrationConfiguration : IEntityTypeConfiguration<CourseRegistration>
{
    public void Configure(EntityTypeBuilder<CourseRegistration> b)
    {
        b.HasKey(x => new { x.UserId, x.GenerationId });
        b.Property(x => x.Status).HasConversion<byte>();
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        b.HasOne(x => x.Generation).WithMany(g => g.Registrations).HasForeignKey(x => x.GenerationId);
    }
}

public class CourseCommentConfiguration : IEntityTypeConfiguration<CourseComment>
{
    public void Configure(EntityTypeBuilder<CourseComment> b)
    {
        b.Property(x => x.Body).HasMaxLength(1000);
        b.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId);
        b.HasOne(x => x.Generation).WithMany(g => g.Comments).HasForeignKey(x => x.GenerationId);
    }
}

public class SyllabusConfiguration : IEntityTypeConfiguration<Syllabus>
{
    public void Configure(EntityTypeBuilder<Syllabus> b)
    {
        b.Property(x => x.NameTH).HasMaxLength(300);
        b.Property(x => x.NameEN).HasMaxLength(300);
        b.Property(x => x.DegreeTH).HasMaxLength(200);
        b.Property(x => x.DegreeEN).HasMaxLength(200);
        b.HasOne(x => x.PdfFile).WithMany().HasForeignKey(x => x.PdfFileId);
    }
}

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> b)
    {
        b.Property(x => x.Code).HasMaxLength(20);
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.Credit).HasPrecision(3, 1);
        b.HasOne(x => x.Syllabus).WithMany(s => s.Subjects).HasForeignKey(x => x.SyllabusId);
    }
}

public class SyllabusObjectiveConfiguration : IEntityTypeConfiguration<SyllabusObjective>
{
    public void Configure(EntityTypeBuilder<SyllabusObjective> b)
    {
        b.Property(x => x.Sentence).HasMaxLength(500);
        b.HasOne(x => x.Syllabus).WithMany(s => s.Objectives).HasForeignKey(x => x.SyllabusId);
    }
}

public class SyllabusOccupationConfiguration : IEntityTypeConfiguration<SyllabusOccupation>
{
    public void Configure(EntityTypeBuilder<SyllabusOccupation> b)
    {
        b.Property(x => x.Sentence).HasMaxLength(500);
        b.HasOne(x => x.Syllabus).WithMany(s => s.Occupations).HasForeignKey(x => x.SyllabusId);
    }
}
