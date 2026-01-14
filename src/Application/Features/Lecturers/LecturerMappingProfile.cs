using AutoMapper;
using Domain.Entities;

namespace Application.Features.Lecturers
{
    public class LecturerMappingProfile : Profile
    {
        public LecturerMappingProfile()
        {
            CreateMap<Lecturer, LecturerDto>();
            CreateMap<Lecturer, LecturerListDto>();
        }
    }
}
