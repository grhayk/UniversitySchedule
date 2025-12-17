using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class GroupSubjectWithStaffConfiguration : BaseEntityConfiguration<GroupSubjectWithLecturer>
    {
        public override void Configure(EntityTypeBuilder<GroupSubjectWithLecturer> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.LessonType).HasConversion<byte>();

            builder.HasOne(x => x.StaffSubject)
                .WithMany(x => x.GroupSubjectsWithStaff)
                .HasForeignKey(x => x.StaffSubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Group)
                .WithMany(x => x.GroupSubjectsWithStaff)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.StaffSubjectId, x.GroupId }).IsUnique();
        }
    }
}
