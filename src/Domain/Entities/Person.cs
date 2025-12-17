namespace Domain.Entities
{
    public abstract class Person : BaseEntity
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public int StructureId { get; set; }
        public DateTime BirthDate { get; set; }

        // Relationships
        public Structure Structure { get; set; } = null!;
    }
}
