using Application.Features.Classrooms;
using Application.Features.Classrooms.CreateClassroom;
using Application.Features.Classrooms.UpdateClassroom;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class ClassroomMappingProfile : Profile
    {
        public ClassroomMappingProfile()
        {
            // Entity to DTO
            CreateMap<Classroom, ClassroomDto>();
            CreateMap<ClassroomCharacteristics, ClassroomCharacteristicsDto>();

            // Command to Entity
            CreateMap<CreateClassroomCommand, Classroom>();
            CreateMap<CreateClassroomCharacteristicsDto, ClassroomCharacteristics>();

            CreateMap<UpdateClassroomCommand, Classroom>();
            CreateMap<UpdateClassroomCharacteristicsDto, ClassroomCharacteristics>();
        }
    }
}
