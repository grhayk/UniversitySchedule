namespace Domain.Entities
{
    public class LecturerSubject : BaseEntity
    {
        public int LecturerId { get; set; }
        public int SubjectId { get; set; }

        // Relationships
        public Lecturer Lecturer { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
        public ICollection<GroupSubjectWithLecturer> GroupSubjectsWithLecturer { get; set; } = new List<GroupSubjectWithLecturer>();
    }
}
