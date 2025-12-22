using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class GroupConfiguration : BaseEntityConfiguration<Group>
    {
        public override void Configure(EntityTypeBuilder<Group> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BranchedFromGroup)
                .WithMany()
                .HasForeignKey(x => x.BranchedFromGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EducationProgram)
                .WithMany(x => x.Groups)
                .HasForeignKey(x => x.EducationProgramId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Semester)
                .WithMany(x => x.Groups)
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
