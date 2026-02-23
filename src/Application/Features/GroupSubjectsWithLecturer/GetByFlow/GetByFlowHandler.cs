using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.GroupSubjectsWithLecturer.GetByFlow
{
    internal class GetByFlowHandler : IRequestHandler<GetByFlowQuery, Result<List<GroupLecturerSubjectListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetByFlowHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<GroupLecturerSubjectListDto>>> Handle(GetByFlowQuery request, CancellationToken ct)
        {
            var flowExists = await _context.Flows.AnyAsync(f => f.Id == request.FlowId, ct);
            if (!flowExists)
            {
                return Result.Failure<List<GroupLecturerSubjectListDto>>(ErrorType.NotFound,
                    $"Flow with ID {request.FlowId} not found.");
            }

            var query = _context.GroupSubjectsWithLecturer
                .Where(g => g.FlowId == request.FlowId)
                .AsNoTracking();

            if (request.LessonType.HasValue)
                query = query.Where(g => g.LessonType == request.LessonType);

            var items = await query
                .OrderBy(g => g.LessonType)
                .ThenBy(g => g.LecturerSubjectId)
                .ToListAsync(ct);

            return Result.Success(_mapper.Map<List<GroupLecturerSubjectListDto>>(items));
        }
    }
}
