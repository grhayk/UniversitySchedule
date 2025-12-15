using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ClassroomCharacteristicsConfiguration : IEntityTypeConfiguration<ClassroomCharacteristics>
    {
        public void Configure(EntityTypeBuilder<ClassroomCharacteristics> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type).HasConversion<byte>();
            builder.Property(x => x.RenovationStatus).HasConversion<byte>();
            builder.Property(x => x.BlackboardCondition).HasConversion<byte>();
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(x => x.Classroom)
                .WithOne(x => x.Characteristics)
                .HasForeignKey<ClassroomCharacteristics>(x => x.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
