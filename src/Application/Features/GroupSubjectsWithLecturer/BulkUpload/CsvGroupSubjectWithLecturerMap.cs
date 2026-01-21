using CsvHelper.Configuration;

namespace Application.Features.GroupSubjectsWithLecturer.BulkUpload
{
    public class CsvGroupSubjectWithLecturerMap : ClassMap<CsvGroupSubjectWithLecturerRecord>
    {
        public CsvGroupSubjectWithLecturerMap()
        {
            Map(m => m.LecturerSubjectId).Index(0).Name("LecturerSubjectId");
            Map(m => m.GroupId).Index(1).Name("GroupId");
            Map(m => m.LessonType).Index(2).Name("LessonType");
        }
    }
}
