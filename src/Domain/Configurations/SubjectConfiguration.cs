using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class SubjectConfiguration : BaseEntityConfiguration<Subject>
    {
        public override void Configure(EntityTypeBuilder<Subject> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);

            builder.HasOne(x => x.SemesterFrom)
                .WithMany(x => x.Subjects)
                .HasForeignKey(x => x.SemesterIdFrom)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SemesterTo)
                .WithMany()
                .HasForeignKey(x => x.SemesterIdTo)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Structure)
                .WithMany(x => x.Subjects)
                .HasForeignKey(x => x.StructureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
