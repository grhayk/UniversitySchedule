using AutoMapper;
using Domain.Entities;

namespace Application.Features.Groups
{
    public class GroupMappingProfile : Profile
    {
        public GroupMappingProfile()
        {
            CreateMap<Group, GroupDto>()
                .ForMember(d => d.ParentName, opt => opt.MapFrom(s => s.Parent != null
                    ? $"{s.Parent.EducationProgram.Code}-{s.Parent.LessonType}-{s.Parent.IndexNumber}"
                    : null))
                .ForMember(d => d.EducationProgramCode, opt => opt.MapFrom(s => s.EducationProgram.Code))
                .ForMember(d => d.EducationProgramName, opt => opt.MapFrom(s => s.EducationProgram.Name))
                .ForMember(d => d.SemesterNumber, opt => opt.MapFrom(s => s.Semester.Number));

            CreateMap<Group, GroupChildDto>();

            CreateMap<Group, GroupListDto>()
                .ForMember(d => d.EducationProgramCode, opt => opt.MapFrom(s => s.EducationProgram.Code))
                .ForMember(d => d.SemesterNumber, opt => opt.MapFrom(s => s.Semester.Number))
                .ForMember(d => d.ChildrenCount, opt => opt.MapFrom(s => s.Children.Count));
        }
    }
}
