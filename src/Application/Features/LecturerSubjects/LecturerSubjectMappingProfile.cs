using AutoMapper;
using Domain.Entities;

namespace Application.Features.LecturerSubjects
{
    public class LecturerSubjectMappingProfile : Profile
    {
        public LecturerSubjectMappingProfile()
        {
            CreateMap<LecturerSubject, LecturerSubjectDto>();
            CreateMap<LecturerSubject, LecturerSubjectListDto>();
            CreateMap<LecturerSubject, SubjectLecturerListDto>();
        }
    }
}
