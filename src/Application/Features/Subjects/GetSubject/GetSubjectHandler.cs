using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Subjects.GetSubject
{
    internal class GetSubjectHandler : IRequestHandler<GetSubjectQuery, Result<SubjectDto>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetSubjectHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<SubjectDto>> Handle(GetSubjectQuery request, CancellationToken ct)
        {
            var subject = await _context.Subjects
                .Include(s => s.SubjectConfigs)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

            if (subject is null)
            {
                return Result.Failure<SubjectDto>(ErrorType.NotFound, $"Subject with ID {request.Id} not found.");
            }

            var dto = _mapper.Map<SubjectDto>(subject);
            return Result.Success(dto);
        }
    }
}
