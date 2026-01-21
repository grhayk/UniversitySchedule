using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Schedules.GetScheduleById
{
    public class GetScheduleByIdHandler : IRequestHandler<GetScheduleByIdQuery, Result<ScheduleDetailDto>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetScheduleByIdHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<ScheduleDetailDto>> Handle(GetScheduleByIdQuery request, CancellationToken ct)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Subject)
                .Include(s => s.TimeTable)
                .Include(s => s.Classroom)
                .Include(s => s.Lecturer)
                .Include(s => s.ScheduleGroups)
                    .ThenInclude(sg => sg.Group)
                .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

            if (schedule is null)
            {
                return Result.Failure<ScheduleDetailDto>(ErrorType.NotFound,
                    $"Schedule with ID {request.Id} not found.");
            }

            var dto = _mapper.Map<ScheduleDetailDto>(schedule);
            return Result.Success(dto);
        }
    }
}
