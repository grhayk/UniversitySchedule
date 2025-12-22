using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class ScheduleGroupConfiguration : BaseEntityConfiguration<ScheduleGroup>
    {
        public override void Configure(EntityTypeBuilder<ScheduleGroup> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.Schedule)
                .WithMany(x => x.ScheduleGroups)
                .HasForeignKey(x => x.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Group)
                .WithMany(x => x.ScheduleGroups)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.ScheduleId, x.GroupId }).IsUnique();
        }
    }
}
