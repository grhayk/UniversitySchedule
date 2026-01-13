using CsvHelper.Configuration;

namespace Application.Features.Subjects.BulkUpload
{
    public class CsvSubjectMap : ClassMap<CsvSubjectRecord>
    {
        public CsvSubjectMap()
        {
            Map(m => m.Code).Index(0).Name("Code");
            Map(m => m.Name).Index(1).Name("Name");
            Map(m => m.SemesterIdFrom).Index(2).Name("SemesterIdFrom");
            Map(m => m.SemesterIdTo).Index(3).Name("SemesterIdTo");
            Map(m => m.StructureId).Index(4).Name("StructureId");
            Map(m => m.LectureHours).Index(5).Name("LectureHours").Optional();
            Map(m => m.PracticalHours).Index(6).Name("PracticalHours").Optional();
            Map(m => m.LaboratoryHours).Index(7).Name("LaboratoryHours").Optional();
        }
    }
}
