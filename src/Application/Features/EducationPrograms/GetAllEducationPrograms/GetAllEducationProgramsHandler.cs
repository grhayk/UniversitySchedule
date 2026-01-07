using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EducationPrograms.GetAllEducationPrograms
{
    public class GetAllEducationProgramsHandler : IRequestHandler<GetAllEducationProgramsQuery, Result<PagedResult<EducationProgramDto>>>
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetAllEducationProgramsHandler(IDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<EducationProgramDto>>> Handle(GetAllEducationProgramsQuery request, CancellationToken ct)
        {
            var query = _dbContext.EducationPrograms.AsNoTracking();

            // Apply filters
            if (request.StructureId.HasValue)
                query = query.Where(p => p.StructureId == request.StructureId);

            if (request.ParentId.HasValue)
                query = query.Where(p => p.ParentId == request.ParentId);

            // Get total count before pagination
            var totalCount = await query.CountAsync(ct);

            // Apply pagination
            var programs = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var result = new PagedResult<EducationProgramDto>
            {
                Items = _mapper.Map<List<EducationProgramDto>>(programs),
                TotalCount = totalCount
            };

            return Result.Success(result);
        }
    }
}
