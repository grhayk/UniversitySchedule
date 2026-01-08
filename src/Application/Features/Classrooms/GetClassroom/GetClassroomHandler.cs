using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Classrooms.GetClassroom
{
    internal class GetClassroomHandler : IRequestHandler<GetClassroomQuery, Result<ClassroomDto>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetClassroomHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<ClassroomDto>> Handle(GetClassroomQuery request, CancellationToken ct)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.Characteristics)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.Id, ct);

            if (classroom is null)
            {
                return Result.Failure<ClassroomDto>(ErrorType.NotFound, $"Classroom with ID {request.Id} not found.");
            }

            var dto = _mapper.Map<ClassroomDto>(classroom);
            return Result.Success(dto);
        }
    }
}
