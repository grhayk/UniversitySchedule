using CsvHelper.Configuration;

namespace Application.Features.Groups.BulkUpload
{
    public class CsvGroupMap : ClassMap<CsvGroupRecord>
    {
        public CsvGroupMap()
        {
            Map(m => m.ParentId).Index(0).Name("ParentId").Optional();
            Map(m => m.EducationProgramId).Index(1).Name("EducationProgramId");
            Map(m => m.SemesterId).Index(2).Name("SemesterId");
            Map(m => m.LessonType).Index(3).Name("LessonType");
            Map(m => m.IsActive).Index(4).Name("IsActive");
            Map(m => m.StartDate).Index(5).Name("StartDate");
            Map(m => m.IndexNumber).Index(6).Name("IndexNumber");
            Map(m => m.BranchedFromGroupId).Index(7).Name("BranchedFromGroupId").Optional();
        }
    }
}
