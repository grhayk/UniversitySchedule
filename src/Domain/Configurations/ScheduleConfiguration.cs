using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ScheduleConfiguration : BaseEntityConfiguration<Schedule>
    {
        public override void Configure(EntityTypeBuilder<Schedule> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.LessonTypeId).HasConversion<byte>();
            builder.Property(x => x.WeekType).HasConversion<byte>();

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.Schedules)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TimeTable)
                .WithMany(x => x.Schedules)
                .HasForeignKey(x => x.TimeTableId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Classroom)
                .WithMany(x => x.Schedules)
                .HasForeignKey(x => x.ClassroomId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.Schedules)
                .HasForeignKey(x => x.StaffId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ScheduleParent)
                .WithMany(x => x.ScheduleExceptions)
                .HasForeignKey(x => x.ScheduleParentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Semester)
                .WithMany(x => x.Schedules)
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
