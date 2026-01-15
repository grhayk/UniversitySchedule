using CsvHelper.Configuration;

namespace Application.Features.SubjectClassrooms.BulkUpload
{
    public class CsvSubjectClassroomMap : ClassMap<CsvSubjectClassroomRecord>
    {
        public CsvSubjectClassroomMap()
        {
            Map(m => m.SubjectId).Index(0).Name("SubjectId");
            Map(m => m.LessonType).Index(1).Name("LessonType");
            Map(m => m.ClassroomId).Index(2).Name("ClassroomId");
        }
    }
}
