using CsvHelper.Configuration;

namespace Application.Features.Classrooms.BulkUpload
{
    public class CsvClassroomMap : ClassMap<CsvClassroomRecord>
    {
        public CsvClassroomMap()
        {
            Map(m => m.Name).Index(0).Name("Name");
            Map(m => m.StructureId).Index(1).Name("StructureId");
            Map(m => m.Type).Index(2).Name("Type");
            Map(m => m.SeatCapacity).Index(3).Name("SeatCapacity");
            Map(m => m.HasComputer).Index(4).Name("HasComputer");
            Map(m => m.ComputerCount).Index(5).Name("ComputerCount").Optional();
            Map(m => m.HasProjector).Index(6).Name("HasProjector");
            Map(m => m.RenovationStatus).Index(7).Name("RenovationStatus");
            Map(m => m.BlackboardCondition).Index(8).Name("BlackboardCondition");
        }
    }
}
