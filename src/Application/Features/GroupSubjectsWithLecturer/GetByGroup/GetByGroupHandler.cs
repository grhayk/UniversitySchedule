using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.GroupSubjectsWithLecturer.GetByGroup
{
    internal class GetByGroupHandler : IRequestHandler<GetByGroupQuery, Result<List<GroupLecturerSubjectListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetByGroupHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<GroupLecturerSubjectListDto>>> Handle(GetByGroupQuery request, CancellationToken ct)
        {
            var query = _context.GroupSubjectsWithLecturer
                .Where(g => g.GroupId == request.GroupId)
                .AsNoTracking();

            if (request.LessonType.HasValue)
                query = query.Where(g => g.LessonType == request.LessonType);

            var items = await query
                .OrderBy(g => g.LessonType)
                .ThenBy(g => g.LecturerSubjectId)
                .ToListAsync(ct);

            return Result.Success(_mapper.Map<List<GroupLecturerSubjectListDto>>(items));
        }
    }
}
