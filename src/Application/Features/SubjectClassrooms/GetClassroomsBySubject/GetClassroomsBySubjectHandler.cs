using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.SubjectClassrooms.GetClassroomsBySubject
{
    internal class GetClassroomsBySubjectHandler : IRequestHandler<GetClassroomsBySubjectQuery, Result<List<SubjectClassroomListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetClassroomsBySubjectHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<SubjectClassroomListDto>>> Handle(GetClassroomsBySubjectQuery request, CancellationToken ct)
        {
            var query = _context.SubjectClassrooms
                .Where(sc => sc.SubjectId == request.SubjectId)
                .AsNoTracking();

            if (request.LessonType.HasValue)
                query = query.Where(sc => sc.LessonType == request.LessonType);

            var subjectClassrooms = await query
                .OrderBy(sc => sc.LessonType)
                .ThenBy(sc => sc.ClassroomId)
                .ToListAsync(ct);

            return Result.Success(_mapper.Map<List<SubjectClassroomListDto>>(subjectClassrooms));
        }
    }
}
