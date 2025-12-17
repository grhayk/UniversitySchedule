using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ClassroomConfiguration : BaseEntityConfiguration<Classroom>
    {
        public override void Configure(EntityTypeBuilder<Classroom> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(20);

            builder.HasOne(x => x.Structure)
                .WithMany(x => x.Classrooms)
                .HasForeignKey(x => x.StructureId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
