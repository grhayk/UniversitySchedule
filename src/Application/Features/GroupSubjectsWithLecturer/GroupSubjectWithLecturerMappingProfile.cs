using AutoMapper;
using Domain.Entities;

namespace Application.Features.GroupSubjectsWithLecturer
{
    public class GroupSubjectWithLecturerMappingProfile : Profile
    {
        public GroupSubjectWithLecturerMappingProfile()
        {
            CreateMap<GroupSubjectWithLecturer, GroupSubjectWithLecturerDto>();
            CreateMap<GroupSubjectWithLecturer, GroupLecturerSubjectListDto>();
            CreateMap<GroupSubjectWithLecturer, LecturerSubjectGroupListDto>();
        }
    }
}
