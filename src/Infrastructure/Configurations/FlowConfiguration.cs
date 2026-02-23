using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class FlowConfiguration : BaseEntityConfiguration<Flow>
    {
        public override void Configure(EntityTypeBuilder<Flow> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.LessonType).HasConversion<byte>();
            builder.Property(x => x.StudentsCount).IsRequired();
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Semester)
                .WithMany()
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
