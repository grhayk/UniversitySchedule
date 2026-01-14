using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Students.GetAllStudents
{
    internal class GetAllStudentsHandler : IRequestHandler<GetAllStudentsQuery, Result<PagedResult<StudentListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetAllStudentsHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<StudentListDto>>> Handle(GetAllStudentsQuery request, CancellationToken ct)
        {
            var query = _context.Students.AsNoTracking();

            // Filter by group
            if (request.GroupId.HasValue)
                query = query.Where(s => s.GroupId == request.GroupId);

            // Search by name
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(s => s.FirstName.ToLower().Contains(term)
                                      || s.LastName.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync(ct);

            var students = await query
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var result = new PagedResult<StudentListDto>
            {
                Items = _mapper.Map<List<StudentListDto>>(students),
                TotalCount = totalCount
            };

            return Result.Success(result);
        }
    }
}
