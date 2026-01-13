using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EducationProgramSubjects.GetProgramsBySubject
{
    internal class GetProgramsBySubjectHandler : IRequestHandler<GetProgramsBySubjectQuery, Result<PagedResult<SubjectProgramDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetProgramsBySubjectHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<SubjectProgramDto>>> Handle(GetProgramsBySubjectQuery request, CancellationToken ct)
        {
            var query = _context.EducationProgramSubjects
                .Include(eps => eps.EducationProgram)
                .Include(eps => eps.Semester)
                .Where(eps => eps.SubjectId == request.SubjectId)
                .AsNoTracking();

            // Filter active only (ToDate is null or in the future)
            if (request.ActiveOnly)
                query = query.Where(eps => eps.ToDate == null || eps.ToDate > DateTime.UtcNow);

            // Get total count before pagination
            var totalCount = await query.CountAsync(ct);

            // Apply pagination
            var items = await query
                .OrderBy(eps => eps.EducationProgram.Code)
                .ThenBy(eps => eps.Semester.Number)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var result = new PagedResult<SubjectProgramDto>
            {
                Items = _mapper.Map<List<SubjectProgramDto>>(items),
                TotalCount = totalCount
            };

            return Result.Success(result);
        }
    }
}
