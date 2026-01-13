using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Groups.GetGroup
{
    internal class GetGroupHandler : IRequestHandler<GetGroupQuery, Result<GroupDto>>
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;

        public GetGroupHandler(IDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<GroupDto>> Handle(GetGroupQuery request, CancellationToken ct)
        {
            var group = await _context.Groups
                .Include(g => g.Parent)
                    .ThenInclude(p => p!.EducationProgram)
                .Include(g => g.EducationProgram)
                .Include(g => g.Semester)
                .Include(g => g.Children)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == request.Id, ct);

            if (group is null)
            {
                return Result.Failure<GroupDto>(ErrorType.NotFound, $"Group with ID {request.Id} not found.");
            }

            return Result.Success(_mapper.Map<GroupDto>(group));
        }
    }
}
