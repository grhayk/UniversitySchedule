using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class PersonConfiguration : BaseEntityConfiguration<Person>
    {
        public override void Configure(EntityTypeBuilder<Person> builder)
        {
            base.Configure(builder);

            // TPH Setup: This defines the "Strategy" for the whole family
            builder.HasDiscriminator<string>("PersonType")
                   .HasValue<Student>("Student")
                   .HasValue<Lecturer>("Lecturer");

            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.BirthDate).IsRequired();

            builder.HasOne(x => x.Structure)
                .WithMany()
                .HasForeignKey(x => x.StructureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
