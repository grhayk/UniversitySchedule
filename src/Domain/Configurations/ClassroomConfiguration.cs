using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
    {
        public void Configure(EntityTypeBuilder<Classroom> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(20);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(x => x.Structure)
                .WithMany(x => x.Classrooms)
                .HasForeignKey(x => x.StructureId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
