using CsvHelper.Configuration;

namespace Application.Features.LecturerSubjects.BulkUpload
{
    public class CsvLecturerSubjectMap : ClassMap<CsvLecturerSubjectRecord>
    {
        public CsvLecturerSubjectMap()
        {
            Map(m => m.LecturerId).Index(0).Name("LecturerId");
            Map(m => m.SubjectId).Index(1).Name("SubjectId");
        }
    }
}
