using Application.Features.Subjects;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class SubjectMappingProfile : Profile
    {
        public SubjectMappingProfile()
        {
            // Entity to DTO
            CreateMap<Subject, SubjectDto>()
                .ForMember(dest => dest.Configs, opt => opt.MapFrom(src => src.SubjectConfigs));
            CreateMap<SubjectConfig, SubjectConfigDto>();
        }
    }
}
