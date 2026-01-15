using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.LecturerSubjects.GetLecturersBySubject
{
    internal class GetLecturersBySubjectHandler : IRequestHandler<GetLecturersBySubjectQuery, Result<List<SubjectLecturerListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetLecturersBySubjectHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<SubjectLecturerListDto>>> Handle(GetLecturersBySubjectQuery request, CancellationToken ct)
        {
            var lecturerSubjects = await _context.LecturerSubjects
                .Where(ls => ls.SubjectId == request.SubjectId)
                .OrderBy(ls => ls.LecturerId)
                .AsNoTracking()
                .ToListAsync(ct);

            return Result.Success(_mapper.Map<List<SubjectLecturerListDto>>(lecturerSubjects));
        }
    }
}
