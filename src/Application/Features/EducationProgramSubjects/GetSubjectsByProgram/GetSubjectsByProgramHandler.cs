using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EducationProgramSubjects.GetSubjectsByProgram
{
    internal class GetSubjectsByProgramHandler : IRequestHandler<GetSubjectsByProgramQuery, Result<PagedResult<ProgramSubjectDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetSubjectsByProgramHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<ProgramSubjectDto>>> Handle(GetSubjectsByProgramQuery request, CancellationToken ct)
        {
            var query = _context.EducationProgramSubjects
                .Include(eps => eps.Subject)
                .Include(eps => eps.Semester)
                .Where(eps => eps.EducationProgramId == request.EducationProgramId)
                .AsNoTracking();

            // Filter by semester
            if (request.SemesterId.HasValue)
                query = query.Where(eps => eps.SemesterId == request.SemesterId);

            // Filter active only (ToDate is null or in the future)
            if (request.ActiveOnly)
                query = query.Where(eps => eps.ToDate == null || eps.ToDate > DateTime.UtcNow);

            // Get total count before pagination
            var totalCount = await query.CountAsync(ct);

            // Apply pagination
            var items = await query
                .OrderBy(eps => eps.Semester.Number)
                .ThenBy(eps => eps.Subject.Code)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var result = new PagedResult<ProgramSubjectDto>
            {
                Items = _mapper.Map<List<ProgramSubjectDto>>(items),
                TotalCount = totalCount
            };

            return Result.Success(result);
        }
    }
}
