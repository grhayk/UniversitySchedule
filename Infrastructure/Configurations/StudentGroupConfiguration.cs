using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class StudentGroupConfiguration : BaseEntityConfiguration<StudentGroup>
    {
        public override void Configure(EntityTypeBuilder<StudentGroup> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.Student)
                .WithMany(x => x.StudentGroups)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Group)
                .WithMany(x => x.StudentGroups)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Semester)
                .WithMany(x => x.StudentGroups)
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.StudentId, x.GroupId, x.SemesterId }).IsUnique();
        }
    }
}
