using CsvHelper.Configuration;

namespace Application.Features.EducationPrograms.BulkUpload
{
    public class CsvEducationProgramMap : ClassMap<CsvEducationProgramRecord>
    {
        public CsvEducationProgramMap()
        {
            Map(m => m.Code).Index(0).Name("Code");
            Map(m => m.Name).Index(1).Name("Name");
            Map(m => m.StructureId).Index(2).Name("StructureId");
            Map(m => m.ParentId).Index(3).Name("ParentId").Optional();
        }
    }
}
