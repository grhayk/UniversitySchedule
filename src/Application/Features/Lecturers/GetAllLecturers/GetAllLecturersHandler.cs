using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Lecturers.GetAllLecturers
{
    internal class GetAllLecturersHandler : IRequestHandler<GetAllLecturersQuery, Result<PagedResult<LecturerListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetAllLecturersHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<LecturerListDto>>> Handle(GetAllLecturersQuery request, CancellationToken ct)
        {
            var query = _context.Lecturers.AsNoTracking();

            // Filter by structure (chair)
            if (request.StructureId.HasValue)
                query = query.Where(l => l.StructureId == request.StructureId);

            // Search by name
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(l => l.FirstName.ToLower().Contains(term)
                                      || l.LastName.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync(ct);

            var lecturers = await query
                .OrderBy(l => l.LastName)
                .ThenBy(l => l.FirstName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var result = new PagedResult<LecturerListDto>
            {
                Items = _mapper.Map<List<LecturerListDto>>(lecturers),
                TotalCount = totalCount
            };

            return Result.Success(result);
        }
    }
}
