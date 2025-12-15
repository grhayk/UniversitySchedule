using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

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
