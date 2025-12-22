using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class GroupSubjectWithLecturerConfiguration : BaseEntityConfiguration<GroupSubjectWithLecturer>
    {
        public override void Configure(EntityTypeBuilder<GroupSubjectWithLecturer> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.LessonType).HasConversion<byte>();

            builder.HasOne(x => x.LecturerSubject)
                .WithMany(x => x.GroupSubjectsWithLecturer)
                .HasForeignKey(x => x.LecturerSubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Group)
                .WithMany(x => x.GroupSubjectsWithLecturer)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.LecturerSubjectId, x.GroupId }).IsUnique();
        }
    }
}
