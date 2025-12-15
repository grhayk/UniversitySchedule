using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class TimeTableConfiguration : IEntityTypeConfiguration<TimeTable>
    {
        public void Configure(EntityTypeBuilder<TimeTable> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.StartTime).HasColumnType("time");
            builder.Property(x => x.EndTime).HasColumnType("time");
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => new { x.StartTime, x.EndTime }).IsUnique();
        }
    }
}
