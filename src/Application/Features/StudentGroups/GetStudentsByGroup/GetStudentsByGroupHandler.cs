using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.StudentGroups.GetStudentsByGroup
{
    internal class GetStudentsByGroupHandler : IRequestHandler<GetStudentsByGroupQuery, Result<PagedResult<StudentGroupListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetStudentsByGroupHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<StudentGroupListDto>>> Handle(GetStudentsByGroupQuery request, CancellationToken ct)
        {
            var query = _context.StudentGroups
                .Include(sg => sg.Student)
                .Include(sg => sg.Group)
                .Where(sg => sg.GroupId == request.GroupId)
                .AsNoTracking();

            var totalCount = await query.CountAsync(ct);

            var studentGroups = await query
                .OrderBy(sg => sg.Student.LastName)
                .ThenBy(sg => sg.Student.FirstName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var result = new PagedResult<StudentGroupListDto>
            {
                Items = _mapper.Map<List<StudentGroupListDto>>(studentGroups),
                TotalCount = totalCount
            };

            return Result.Success(result);
        }
    }
}
