using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.StudentGroups.GetGroupsByStudent
{
    internal class GetGroupsByStudentHandler : IRequestHandler<GetGroupsByStudentQuery, Result<List<StudentGroupDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetGroupsByStudentHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<StudentGroupDto>>> Handle(GetGroupsByStudentQuery request, CancellationToken ct)
        {
            var studentGroups = await _context.StudentGroups
                .Include(sg => sg.Group)
                .Where(sg => sg.StudentId == request.StudentId)
                .OrderBy(sg => sg.Group.LessonType)
                .ThenBy(sg => sg.Group.IndexNumber)
                .AsNoTracking()
                .ToListAsync(ct);

            return Result.Success(_mapper.Map<List<StudentGroupDto>>(studentGroups));
        }
    }
}
