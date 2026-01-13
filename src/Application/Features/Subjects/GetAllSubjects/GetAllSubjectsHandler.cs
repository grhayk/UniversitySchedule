using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Subjects.GetAllSubjects
{
    internal class GetAllSubjectsHandler : IRequestHandler<GetAllSubjectsQuery, Result<PagedResult<SubjectDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetAllSubjectsHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<SubjectDto>>> Handle(GetAllSubjectsQuery request, CancellationToken ct)
        {
            var query = _context.Subjects
                .Include(s => s.SubjectConfigs)
                .AsNoTracking();

            // Filter by structure
            if (request.StructureId.HasValue)
                query = query.Where(s => s.StructureId == request.StructureId);

            // Filter by semester (subjects that include this semester in their range)
            if (request.SemesterId.HasValue)
                query = query.Where(s => s.SemesterIdFrom <= request.SemesterId && s.SemesterIdTo >= request.SemesterId);

            // Search by code or name
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(s => s.Code.ToLower().Contains(term) || s.Name.ToLower().Contains(term));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(ct);

            // Apply pagination
            var subjects = await query
                .OrderBy(s => s.Code)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var result = new PagedResult<SubjectDto>
            {
                Items = _mapper.Map<List<SubjectDto>>(subjects),
                TotalCount = totalCount
            };

            return Result.Success(result);
        }
    }
}
