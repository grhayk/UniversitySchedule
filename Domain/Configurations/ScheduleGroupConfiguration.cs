using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class ScheduleGroupConfiguration : IEntityTypeConfiguration<ScheduleGroup>
    {
        public void Configure(EntityTypeBuilder<ScheduleGroup> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

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
