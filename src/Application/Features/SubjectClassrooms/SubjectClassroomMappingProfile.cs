using AutoMapper;
using Domain.Entities;

namespace Application.Features.SubjectClassrooms
{
    public class SubjectClassroomMappingProfile : Profile
    {
        public SubjectClassroomMappingProfile()
        {
            CreateMap<SubjectClassroom, SubjectClassroomDto>();
            CreateMap<SubjectClassroom, SubjectClassroomListDto>();
            CreateMap<SubjectClassroom, ClassroomSubjectListDto>();
        }
    }
}
