using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class GroupSubjectWithStaffConfiguration : IEntityTypeConfiguration<GroupSubjectWithStaff>
    {
        public void Configure(EntityTypeBuilder<GroupSubjectWithStaff> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LessonType).HasConversion<byte>();
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

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
