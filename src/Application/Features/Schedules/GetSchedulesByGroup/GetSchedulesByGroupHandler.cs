using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Schedules.GetSchedulesByGroup
{
    public class GetSchedulesByGroupHandler : IRequestHandler<GetSchedulesByGroupQuery, Result<List<ScheduleListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetSchedulesByGroupHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ScheduleListDto>>> Handle(GetSchedulesByGroupQuery request, CancellationToken ct)
        {
            // Validate group exists
            var groupExists = await _context.Groups.AnyAsync(g => g.Id == request.GroupId, ct);
            if (!groupExists)
            {
                return Result.Failure<List<ScheduleListDto>>(ErrorType.NotFound,
                    $"Group with ID {request.GroupId} not found.");
            }

            var query = _context.ScheduleGroups
                .Where(sg => sg.GroupId == request.GroupId)
                .Select(sg => sg.Schedule)
                .Include(s => s.Subject)
                .Include(s => s.TimeTable)
                .Include(s => s.Classroom)
                .Include(s => s.Lecturer)
                .Include(s => s.ScheduleGroups)
                .AsQueryable();

            if (request.DateFrom.HasValue)
                query = query.Where(s => s.ScheduleDate >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
                query = query.Where(s => s.ScheduleDate <= request.DateTo.Value);

            if (request.LessonType.HasValue)
                query = query.Where(s => s.LessonTypeId == request.LessonType.Value);

            if (request.WeekType.HasValue)
                query = query.Where(s => s.WeekType == request.WeekType.Value);

            var schedules = await query
                .OrderBy(s => s.ScheduleDate)
                .ThenBy(s => s.TimeTable.StartTime)
                .ToListAsync(ct);

            var dtos = _mapper.Map<List<ScheduleListDto>>(schedules);
            return Result.Success(dtos);
        }
    }
}
