using Application.Models.ScheduleGeneration;

namespace Application.Interfaces
{
    public interface IScheduleGenerator
    {
        ScheduleGeneratorOutput Generate(ScheduleGeneratorInput input);
    }
}
