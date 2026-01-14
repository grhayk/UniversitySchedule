using AutoMapper;
using Domain.Entities;

namespace Application.Features.Students
{
    public class StudentMappingProfile : Profile
    {
        public StudentMappingProfile()
        {
            CreateMap<Student, StudentDto>();
            CreateMap<Student, StudentListDto>();
        }
    }
}
