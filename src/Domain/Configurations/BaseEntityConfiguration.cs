using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            // 1. Set the Primary Key once for everyone
            builder.HasKey(x => x.Id);

            // 2. Set the default SQL value for CreatedAt
            builder.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
