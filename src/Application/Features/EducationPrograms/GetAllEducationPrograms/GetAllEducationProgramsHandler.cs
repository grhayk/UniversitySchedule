using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EducationPrograms.GetAllEducationPrograms
{
    public class GetAllEducationProgramsHandler : IRequestHandler<GetAllEducationProgramsQuery, Result<List<EducationProgramDto>>>
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetAllEducationProgramsHandler(IDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Result<List<EducationProgramDto>>> Handle(GetAllEducationProgramsQuery request, CancellationToken ct)
        {
            var query = _dbContext.EducationPrograms.AsNoTracking();

            if (request.StructureId.HasValue)
                query = query.Where(p => p.StructureId == request.StructureId);

            if (request.ParentId.HasValue)
                query = query.Where(p => p.ParentId == request.ParentId);

            var programs = await query.ToListAsync(ct);

            return Result.Success(_mapper.Map<List<EducationProgramDto>>(programs));
        }
    }
}
