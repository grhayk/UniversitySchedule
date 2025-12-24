using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EducationPrograms.GetEducationProgram
{
    public class GetEducationProgramHandler : IRequestHandler<GetEducationProgramQuery, Result<EducationProgramDto>>
    {
        private readonly IDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetEducationProgramHandler(IDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Result<EducationProgramDto>> Handle(GetEducationProgramQuery request, CancellationToken ct)
        {
            var program = await _dbContext.EducationPrograms
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (program == null)
            {
                return Result.Failure<EducationProgramDto>(
                    ErrorType.NotFound,
                    $"Education Program with ID {request.Id} not found");
            }

            return Result.Success(_mapper.Map<EducationProgramDto>(program));
        }
    }
}
