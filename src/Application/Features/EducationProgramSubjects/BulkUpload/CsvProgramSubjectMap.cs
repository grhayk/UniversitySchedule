using CsvHelper.Configuration;

namespace Application.Features.EducationProgramSubjects.BulkUpload
{
    public class CsvProgramSubjectMap : ClassMap<CsvProgramSubjectRecord>
    {
        public CsvProgramSubjectMap()
        {
            Map(m => m.EducationProgramId).Index(0).Name("EducationProgramId");
            Map(m => m.SubjectId).Index(1).Name("SubjectId");
            Map(m => m.SemesterId).Index(2).Name("SemesterId");
            Map(m => m.FromDate).Index(3).Name("FromDate");
            Map(m => m.ToDate).Index(4).Name("ToDate").Optional();
        }
    }
}
