using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Groups.GetAllGroups
{
    internal class GetAllGroupsHandler : IRequestHandler<GetAllGroupsQuery, Result<PagedResult<GroupListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetAllGroupsHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<GroupListDto>>> Handle(GetAllGroupsQuery request, CancellationToken ct)
        {
            var query = _context.Groups
                .Include(g => g.EducationProgram)
                .Include(g => g.Semester)
                .Include(g => g.Children)
                .AsNoTracking();

            // Filter by education program
            if (request.EducationProgramId.HasValue)
                query = query.Where(g => g.EducationProgramId == request.EducationProgramId);

            // Filter by semester
            if (request.SemesterId.HasValue)
                query = query.Where(g => g.SemesterId == request.SemesterId);

            // Filter by lesson type
            if (request.LessonType.HasValue)
                query = query.Where(g => g.LessonType == request.LessonType);

            // Filter by active status
            if (request.IsActive.HasValue)
                query = query.Where(g => g.IsActive == request.IsActive);

            // Filter main groups only (no parent)
            if (request.MainGroupsOnly == true)
                query = query.Where(g => g.ParentId == null);

            // Get total count before pagination
            var totalCount = await query.CountAsync(ct);

            // Apply pagination
            var groups = await query
                .OrderBy(g => g.EducationProgram.Code)
                .ThenBy(g => g.Semester.Number)
                .ThenBy(g => g.LessonType)
                .ThenBy(g => g.IndexNumber)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var result = new PagedResult<GroupListDto>
            {
                Items = _mapper.Map<List<GroupListDto>>(groups),
                TotalCount = totalCount
            };

            return Result.Success(result);
        }
    }
}
