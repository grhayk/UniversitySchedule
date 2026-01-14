using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Students.GetStudent
{
    internal class GetStudentHandler : IRequestHandler<GetStudentQuery, Result<StudentDto>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetStudentHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<StudentDto>> Handle(GetStudentQuery request, CancellationToken ct)
        {
            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

            if (student is null)
            {
                return Result.Failure<StudentDto>(ErrorType.NotFound, $"Student with ID {request.Id} not found.");
            }

            return Result.Success(_mapper.Map<StudentDto>(student));
        }
    }
}
