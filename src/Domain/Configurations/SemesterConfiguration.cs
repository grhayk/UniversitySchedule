using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class SemesterConfiguration : IEntityTypeConfiguration<Semester>
    {
        public void Configure(EntityTypeBuilder<Semester> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.EducationDegree).HasConversion<int>();
            builder.Property(x => x.EducationType).HasConversion<int>();
            builder.Property(x => x.Number).IsRequired();
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => new { x.EducationDegree, x.EducationType, x.Number }).IsUnique();
        }
    }
}
