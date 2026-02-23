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
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne(x => x.Flow)
                .WithMany(x => x.GroupSubjectsWithLecturer)
                .HasForeignKey(x => x.FlowId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Exactly one of GroupId or FlowId must be set
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_GroupSubjectWithLecturer_GroupOrFlow",
                "([GroupId] IS NOT NULL AND [FlowId] IS NULL) OR ([GroupId] IS NULL AND [FlowId] IS NOT NULL)"));

            // A lecturer-subject can only be assigned once per group (or flow) with the same lesson type
            builder.HasIndex(x => new { x.LecturerSubjectId, x.GroupId, x.LessonType })
                .IsUnique()
                .HasFilter("[GroupId] IS NOT NULL");

            builder.HasIndex(x => new { x.LecturerSubjectId, x.FlowId, x.LessonType })
                .IsUnique()
                .HasFilter("[FlowId] IS NOT NULL");
        }
    }
}
