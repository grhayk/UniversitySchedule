using CsvHelper.Configuration;

namespace Application.Features.Schedules.BulkUpload
{
    public class CsvScheduleMap : ClassMap<CsvScheduleRecord>
    {
        public CsvScheduleMap()
        {
            Map(m => m.SubjectId).Index(0).Name("SubjectId");
            Map(m => m.LecturerId).Index(1).Name("LecturerId");
            Map(m => m.LessonType).Index(2).Name("LessonType");
            Map(m => m.ClassroomId).Index(3).Name("ClassroomId");
            Map(m => m.TimeTableId).Index(4).Name("TimeTableId");
            Map(m => m.WeekType).Index(5).Name("WeekType");
            Map(m => m.ScheduleDate).Index(6).Name("ScheduleDate");
            Map(m => m.SemesterId).Index(7).Name("SemesterId");
            Map(m => m.ScheduleParentId).Index(8).Name("ScheduleParentId").Optional();
            Map(m => m.GroupIds).Index(9).Name("GroupIds");
        }
    }
}
