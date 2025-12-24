using Application.Features.EducationPrograms;
using Application.Features.EducationPrograms.BulkUpload;
using Application.Features.EducationPrograms.CreateEducationProgram;
using Application.Features.EducationPrograms.UpdateEducationProgram;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class EducationProgramMappingProfile : Profile
    {
        public EducationProgramMappingProfile()
        {
            // Entity to DTO
            CreateMap<EducationProgram, EducationProgramDto>().ReverseMap(); // Allows mapping both ways

            // Command to Entity (if needed)
            CreateMap<CreateEducationProgramCommand, EducationProgram>();
            CreateMap<UpdateEducationProgramCommand, EducationProgram>();

            // CSV Record to Entity
            CreateMap<CsvEducationProgramRecord, EducationProgram>();
        }
    }
}
