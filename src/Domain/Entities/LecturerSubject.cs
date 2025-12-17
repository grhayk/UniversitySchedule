namespace Domain.Entities
{
    public class LecturerSubject : BaseEntity
    {
        public int StaffId { get; set; }
        public int SubjectId { get; set; }

        // Relationships
        public Lecturer Staff { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
        public ICollection<GroupSubjectWithLecturer> GroupSubjectsWithStaff { get; set; } = new List<GroupSubjectWithLecturer>();
    }
}
