using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class LecturerSubjectConfiguration : BaseEntityConfiguration<LecturerSubject>
    {
        public override void Configure(EntityTypeBuilder<LecturerSubject> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.Lecturer)
                .WithMany(x => x.LecturerSubjects)
                .HasForeignKey(x => x.LecturerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.LecturerSubjects)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
