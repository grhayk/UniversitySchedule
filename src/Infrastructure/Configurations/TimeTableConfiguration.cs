using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class TimeTableConfiguration : BaseEntityConfiguration<TimeTable>
    {
        public override void Configure(EntityTypeBuilder<TimeTable> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.StartTime).HasColumnType("time");
            builder.Property(x => x.EndTime).HasColumnType("time");
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => new { x.StartTime, x.EndTime }).IsUnique();
        }
    }
}
