using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.LecturerSubjects.GetSubjectsByLecturer
{
    internal class GetSubjectsByLecturerHandler : IRequestHandler<GetSubjectsByLecturerQuery, Result<List<LecturerSubjectListDto>>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetSubjectsByLecturerHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<LecturerSubjectListDto>>> Handle(GetSubjectsByLecturerQuery request, CancellationToken ct)
        {
            var lecturerSubjects = await _context.LecturerSubjects
                .Where(ls => ls.LecturerId == request.LecturerId)
                .OrderBy(ls => ls.SubjectId)
                .AsNoTracking()
                .ToListAsync(ct);

            return Result.Success(_mapper.Map<List<LecturerSubjectListDto>>(lecturerSubjects));
        }
    }
}
