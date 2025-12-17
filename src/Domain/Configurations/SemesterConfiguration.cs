using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class SemesterConfiguration : BaseEntityConfiguration<Semester>
    {
        public override void Configure(EntityTypeBuilder<Semester> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.EducationDegree).HasConversion<int>();
            builder.Property(x => x.EducationType).HasConversion<int>();
            builder.Property(x => x.Number).IsRequired();

            builder.HasIndex(x => new { x.EducationDegree, x.EducationType, x.Number }).IsUnique();
        }
    }
}
