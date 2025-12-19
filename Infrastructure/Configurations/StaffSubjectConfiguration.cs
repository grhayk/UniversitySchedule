using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class StaffSubjectConfiguration : BaseEntityConfiguration<LecturerSubject>
    {
        public override void Configure(EntityTypeBuilder<LecturerSubject> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.StaffSubjects)
                .HasForeignKey(x => x.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.StaffSubjects)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
