using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

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
