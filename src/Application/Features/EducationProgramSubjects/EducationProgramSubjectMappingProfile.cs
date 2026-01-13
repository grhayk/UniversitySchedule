using AutoMapper;
using Domain.Entities;

namespace Application.Features.EducationProgramSubjects
{
    public class EducationProgramSubjectMappingProfile : Profile
    {
        public EducationProgramSubjectMappingProfile()
        {
            CreateMap<EducationProgramSubject, EducationProgramSubjectDto>()
                .ForMember(d => d.EducationProgramCode, opt => opt.MapFrom(s => s.EducationProgram.Code))
                .ForMember(d => d.EducationProgramName, opt => opt.MapFrom(s => s.EducationProgram.Name))
                .ForMember(d => d.SubjectCode, opt => opt.MapFrom(s => s.Subject.Code))
                .ForMember(d => d.SubjectName, opt => opt.MapFrom(s => s.Subject.Name))
                .ForMember(d => d.SemesterNumber, opt => opt.MapFrom(s => s.Semester.Number));

            CreateMap<EducationProgramSubject, ProgramSubjectDto>()
                .ForMember(d => d.SubjectCode, opt => opt.MapFrom(s => s.Subject.Code))
                .ForMember(d => d.SubjectName, opt => opt.MapFrom(s => s.Subject.Name))
                .ForMember(d => d.SemesterNumber, opt => opt.MapFrom(s => s.Semester.Number));

            CreateMap<EducationProgramSubject, SubjectProgramDto>()
                .ForMember(d => d.EducationProgramCode, opt => opt.MapFrom(s => s.EducationProgram.Code))
                .ForMember(d => d.EducationProgramName, opt => opt.MapFrom(s => s.EducationProgram.Name))
                .ForMember(d => d.SemesterNumber, opt => opt.MapFrom(s => s.Semester.Number));
        }
    }
}
