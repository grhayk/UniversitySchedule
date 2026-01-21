using AutoMapper;
using Domain.Entities;

namespace Application.Features.Schedules
{
    public class ScheduleMappingProfile : Profile
    {
        public ScheduleMappingProfile()
        {
            CreateMap<Schedule, ScheduleDto>()
                .ForMember(d => d.LessonType, opt => opt.MapFrom(s => s.LessonTypeId))
                .ForMember(d => d.GroupIds, opt => opt.MapFrom(s => s.ScheduleGroups.Select(sg => sg.GroupId).ToList()));

            CreateMap<Schedule, ScheduleDetailDto>()
                .ForMember(d => d.LessonType, opt => opt.MapFrom(s => s.LessonTypeId))
                .ForMember(d => d.SubjectName, opt => opt.MapFrom(s => s.Subject.Name))
                .ForMember(d => d.StartTime, opt => opt.MapFrom(s => s.TimeTable.StartTime))
                .ForMember(d => d.EndTime, opt => opt.MapFrom(s => s.TimeTable.EndTime))
                .ForMember(d => d.ClassroomName, opt => opt.MapFrom(s => s.Classroom.Name))
                .ForMember(d => d.LecturerName, opt => opt.MapFrom(s => s.Lecturer.FirstName + " " + s.Lecturer.LastName))
                .ForMember(d => d.Groups, opt => opt.MapFrom(s => s.ScheduleGroups));

            CreateMap<ScheduleGroup, ScheduleGroupDto>()
                .ForMember(d => d.IndexNumber, opt => opt.MapFrom(s => s.Group.IndexNumber))
                .ForMember(d => d.GroupLessonType, opt => opt.MapFrom(s => s.Group.LessonType));

            CreateMap<Schedule, ScheduleListDto>()
                .ForMember(d => d.LessonType, opt => opt.MapFrom(s => s.LessonTypeId))
                .ForMember(d => d.SubjectName, opt => opt.MapFrom(s => s.Subject.Name))
                .ForMember(d => d.StartTime, opt => opt.MapFrom(s => s.TimeTable.StartTime))
                .ForMember(d => d.EndTime, opt => opt.MapFrom(s => s.TimeTable.EndTime))
                .ForMember(d => d.ClassroomName, opt => opt.MapFrom(s => s.Classroom.Name))
                .ForMember(d => d.LecturerName, opt => opt.MapFrom(s => s.Lecturer.FirstName + " " + s.Lecturer.LastName))
                .ForMember(d => d.GroupCount, opt => opt.MapFrom(s => s.ScheduleGroups.Count));
        }
    }
}
