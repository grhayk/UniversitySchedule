using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Schedules.GetSchedulesBySemester
{
    public class GetSchedulesBySemesterHandler : IRequestHandler<GetSchedulesBySemesterQuery, Result<List<ScheduleListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetSchedulesBySemesterHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ScheduleListDto>>> Handle(GetSchedulesBySemesterQuery request, CancellationToken ct)
        {
            // Validate semester exists
            var semesterExists = await _context.Semesters.AnyAsync(s => s.Id == request.SemesterId, ct);
            if (!semesterExists)
            {
                return Result.Failure<List<ScheduleListDto>>(ErrorType.NotFound,
                    $"Semester with ID {request.SemesterId} not found.");
            }

            var query = _context.Schedules
                .Include(s => s.Subject)
                .Include(s => s.TimeTable)
                .Include(s => s.Classroom)
                .Include(s => s.Lecturer)
                .Include(s => s.ScheduleGroups)
                .Where(s => s.SemesterId == request.SemesterId)
                .AsQueryable();

            if (request.DateFrom.HasValue)
                query = query.Where(s => s.ScheduleDate >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
                query = query.Where(s => s.ScheduleDate <= request.DateTo.Value);

            if (request.LessonType.HasValue)
                query = query.Where(s => s.LessonTypeId == request.LessonType.Value);

            if (request.WeekType.HasValue)
                query = query.Where(s => s.WeekType == request.WeekType.Value);

            if (request.LecturerId.HasValue)
                query = query.Where(s => s.LecturerId == request.LecturerId.Value);

            if (request.SubjectId.HasValue)
                query = query.Where(s => s.SubjectId == request.SubjectId.Value);

            var schedules = await query
                .OrderBy(s => s.ScheduleDate)
                .ThenBy(s => s.TimeTable.StartTime)
                .ToListAsync(ct);

            var dtos = _mapper.Map<List<ScheduleListDto>>(schedules);
            return Result.Success(dtos);
        }
    }
}
