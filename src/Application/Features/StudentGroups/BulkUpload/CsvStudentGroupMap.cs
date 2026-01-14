using CsvHelper.Configuration;

namespace Application.Features.StudentGroups.BulkUpload
{
    public class CsvStudentGroupMap : ClassMap<CsvStudentGroupRecord>
    {
        public CsvStudentGroupMap()
        {
            Map(m => m.StudentId).Index(0).Name("StudentId");
            Map(m => m.GroupId).Index(1).Name("GroupId");
        }
    }
}
