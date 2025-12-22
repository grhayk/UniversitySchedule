using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class SubjectConfigConfiguration : BaseEntityConfiguration<SubjectConfig>
    {
        public override void Configure(EntityTypeBuilder<SubjectConfig> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.SubjectConfigs)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
