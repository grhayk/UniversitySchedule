using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Classrooms.GetAllClassrooms
{
    internal class GetAllClassroomsHandler : IRequestHandler<GetAllClassroomsQuery, Result<PagedResult<ClassroomDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetAllClassroomsHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<ClassroomDto>>> Handle(GetAllClassroomsQuery request, CancellationToken ct)
        {
            var query = _context.Classrooms
                .Include(c => c.Characteristics)
                .AsNoTracking();

            // Apply filters
            if (request.StructureId.HasValue)
                query = query.Where(c => c.StructureId == request.StructureId);

            if (request.ClassroomType.HasValue)
                query = query.Where(c => c.Characteristics!.Type == request.ClassroomType);

            if (request.MinSeatCapacity.HasValue)
                query = query.Where(c => c.Characteristics!.SeatCapacity >= request.MinSeatCapacity);

            if (request.MaxSeatCapacity.HasValue)
                query = query.Where(c => c.Characteristics!.SeatCapacity <= request.MaxSeatCapacity);

            if (request.HasComputer.HasValue)
                query = query.Where(c => c.Characteristics!.HasComputer == request.HasComputer);

            if (request.HasProjector.HasValue)
                query = query.Where(c => c.Characteristics!.HasProjector == request.HasProjector);

            // Get total count before pagination
            var totalCount = await query.CountAsync(ct);

            // Apply pagination
            var classrooms = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var result = new PagedResult<ClassroomDto>
            {
                Items = _mapper.Map<List<ClassroomDto>>(classrooms),
                TotalCount = totalCount
            };

            return Result.Success(result);
        }
    }
}
