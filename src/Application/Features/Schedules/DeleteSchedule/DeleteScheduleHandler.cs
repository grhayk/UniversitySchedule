using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Schedules.DeleteSchedule
{
    public class DeleteScheduleHandler : IRequestHandler<DeleteScheduleCommand, Result>
    {
        private readonly IDbContext _context;

        public DeleteScheduleHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeleteScheduleCommand request, CancellationToken ct)
        {
            var schedule = await _context.Schedules
                .Include(s => s.ScheduleGroups)
                .Include(s => s.ScheduleExceptions)
                .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

            if (schedule is null)
            {
                return Result.Failure(ErrorType.NotFound,
                    $"Schedule with ID {request.Id} not found.");
            }

            // Check if this schedule has child schedules (exceptions)
            if (schedule.ScheduleExceptions.Any())
            {
                return Result.Failure(ErrorType.Validation,
                    $"Cannot delete schedule with ID {request.Id} because it has {schedule.ScheduleExceptions.Count} child schedule(s). Delete the child schedules first.");
            }

            // Remove ScheduleGroups first
            _context.ScheduleGroups.RemoveRange(schedule.ScheduleGroups);

            // Remove Schedule
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync(ct);

            return Result.Success("Schedule deleted successfully.");
        }
    }
}
