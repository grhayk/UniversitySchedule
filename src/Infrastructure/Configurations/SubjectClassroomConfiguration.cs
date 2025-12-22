using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class SubjectClassroomConfiguration : BaseEntityConfiguration<SubjectClassroom>
    {
        public override void Configure(EntityTypeBuilder<SubjectClassroom> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.SubjectClassrooms)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Classroom)
                .WithMany(x => x.SubjectClassrooms)
                .HasForeignKey(x => x.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
