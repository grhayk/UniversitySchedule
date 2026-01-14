using AutoMapper;
using Domain.Entities;

namespace Application.Features.StudentGroups
{
    public class StudentGroupMappingProfile : Profile
    {
        public StudentGroupMappingProfile()
        {
            CreateMap<StudentGroup, StudentGroupDto>()
                .ForMember(d => d.GroupLessonType, opt => opt.MapFrom(s => s.Group.LessonType))
                .ForMember(d => d.GroupIndexNumber, opt => opt.MapFrom(s => s.Group.IndexNumber));

            CreateMap<StudentGroup, StudentGroupListDto>()
                .ForMember(d => d.StudentFirstName, opt => opt.MapFrom(s => s.Student.FirstName))
                .ForMember(d => d.StudentLastName, opt => opt.MapFrom(s => s.Student.LastName))
                .ForMember(d => d.GroupLessonType, opt => opt.MapFrom(s => s.Group.LessonType))
                .ForMember(d => d.GroupIndexNumber, opt => opt.MapFrom(s => s.Group.IndexNumber));
        }
    }
}
