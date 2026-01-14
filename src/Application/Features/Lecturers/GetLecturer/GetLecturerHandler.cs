using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Lecturers.GetLecturer
{
    internal class GetLecturerHandler : IRequestHandler<GetLecturerQuery, Result<LecturerDto>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetLecturerHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<LecturerDto>> Handle(GetLecturerQuery request, CancellationToken ct)
        {
            var lecturer = await _context.Lecturers
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == request.Id, ct);

            if (lecturer is null)
            {
                return Result.Failure<LecturerDto>(ErrorType.NotFound, $"Lecturer with ID {request.Id} not found.");
            }

            return Result.Success(_mapper.Map<LecturerDto>(lecturer));
        }
    }
}
