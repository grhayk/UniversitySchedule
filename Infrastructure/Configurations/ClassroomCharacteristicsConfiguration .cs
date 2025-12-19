using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class ClassroomCharacteristicsConfiguration : BaseEntityConfiguration<ClassroomCharacteristics>
    {
        public override void Configure(EntityTypeBuilder<ClassroomCharacteristics> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Type).HasConversion<byte>();
            builder.Property(x => x.RenovationStatus).HasConversion<byte>();
            builder.Property(x => x.BlackboardCondition).HasConversion<byte>();

            builder.HasOne(x => x.Classroom)
                .WithOne(x => x.Characteristics)
                .HasForeignKey<ClassroomCharacteristics>(x => x.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
