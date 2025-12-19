using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class EducationProgramSubjectConfiguration : BaseEntityConfiguration<EducationProgramSubject>
    {
        public override void Configure(EntityTypeBuilder<EducationProgramSubject> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.EducationProgram)
                .WithMany(x => x.EducationProgramSubjects)
                .HasForeignKey(x => x.EducationProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.EducationProgramSubjects)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Semester)
                .WithMany()
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.EducationProgramId, x.SubjectId, x.SemesterId }).IsUnique();
        }
    }
}