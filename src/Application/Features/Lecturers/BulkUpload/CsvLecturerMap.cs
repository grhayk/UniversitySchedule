using CsvHelper.Configuration;

namespace Application.Features.Lecturers.BulkUpload
{
    public class CsvLecturerMap : ClassMap<CsvLecturerRecord>
    {
        public CsvLecturerMap()
        {
            Map(m => m.FirstName).Index(0).Name("FirstName");
            Map(m => m.LastName).Index(1).Name("LastName");
            Map(m => m.BirthDate).Index(2).Name("BirthDate");
            Map(m => m.StructureId).Index(3).Name("StructureId");
        }
    }
}
