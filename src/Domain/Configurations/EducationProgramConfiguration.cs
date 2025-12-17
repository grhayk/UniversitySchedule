using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class EducationProgramConfiguration : BaseEntityConfiguration<EducationProgram>
    {
        public override void Configure(EntityTypeBuilder<EducationProgram> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);

            builder.HasOne(x => x.Structure)
                .WithMany(x => x.EducationPrograms)
                .HasForeignKey(x => x.StructureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
