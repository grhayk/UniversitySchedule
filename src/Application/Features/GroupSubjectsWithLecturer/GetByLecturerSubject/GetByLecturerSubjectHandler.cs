using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.GroupSubjectsWithLecturer.GetByLecturerSubject
{
    internal class GetByLecturerSubjectHandler : IRequestHandler<GetByLecturerSubjectQuery, Result<List<LecturerSubjectGroupListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetByLecturerSubjectHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<LecturerSubjectGroupListDto>>> Handle(GetByLecturerSubjectQuery request, CancellationToken ct)
        {
            var query = _context.GroupSubjectsWithLecturer
                .Where(g => g.LecturerSubjectId == request.LecturerSubjectId)
                .AsNoTracking();

            if (request.LessonType.HasValue)
                query = query.Where(g => g.LessonType == request.LessonType);

            var items = await query
                .OrderBy(g => g.GroupId)
                .ThenBy(g => g.LessonType)
                .ToListAsync(ct);

            return Result.Success(_mapper.Map<List<LecturerSubjectGroupListDto>>(items));
        }
    }
}
