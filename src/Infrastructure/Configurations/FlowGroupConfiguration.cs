using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class FlowGroupConfiguration : BaseEntityConfiguration<FlowGroup>
    {
        public override void Configure(EntityTypeBuilder<FlowGroup> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.Flow)
                .WithMany(x => x.FlowGroups)
                .HasForeignKey(x => x.FlowId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Group)
                .WithMany(x => x.FlowGroups)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // A group can only appear once per flow
            builder.HasIndex(x => new { x.FlowId, x.GroupId }).IsUnique();
        }
    }
}
