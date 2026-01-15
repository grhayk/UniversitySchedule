using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.SubjectClassrooms.GetSubjectsByClassroom
{
    internal class GetSubjectsByClassroomHandler : IRequestHandler<GetSubjectsByClassroomQuery, Result<List<ClassroomSubjectListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetSubjectsByClassroomHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ClassroomSubjectListDto>>> Handle(GetSubjectsByClassroomQuery request, CancellationToken ct)
        {
            var query = _context.SubjectClassrooms
                .Where(sc => sc.ClassroomId == request.ClassroomId)
                .AsNoTracking();

            if (request.LessonType.HasValue)
                query = query.Where(sc => sc.LessonType == request.LessonType);

            var subjectClassrooms = await query
                .OrderBy(sc => sc.SubjectId)
                .ThenBy(sc => sc.LessonType)
                .ToListAsync(ct);

            return Result.Success(_mapper.Map<List<ClassroomSubjectListDto>>(subjectClassrooms));
        }
    }
}
