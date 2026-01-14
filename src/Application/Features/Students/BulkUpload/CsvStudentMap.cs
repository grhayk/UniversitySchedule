using CsvHelper.Configuration;

namespace Application.Features.Students.BulkUpload
{
    public class CsvStudentMap : ClassMap<CsvStudentRecord>
    {
        public CsvStudentMap()
        {
            Map(m => m.FirstName).Index(0).Name("FirstName");
            Map(m => m.LastName).Index(1).Name("LastName");
            Map(m => m.BirthDate).Index(2).Name("BirthDate");
            Map(m => m.GroupId).Index(3).Name("GroupId");
        }
    }
}
