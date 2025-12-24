using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EducationPrograms.DeleteEducationProgram
{
    public class DeleteEducationProgramHandler : IRequestHandler<DeleteEducationProgramCommand, Result>
    {
        private readonly IDbContext _dbContext;

        public DeleteEducationProgramHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result> Handle(DeleteEducationProgramCommand request, CancellationToken ct)
        {
            var program = await _dbContext.EducationPrograms.FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (program == null)
            {
                return Result.Failure(ErrorType.NotFound, $"Education Program with ID {request.Id} not found");
            }

            // Check if it has child programs
            var hasChildren = await _dbContext.EducationPrograms.AnyAsync(p => p.ParentId == request.Id, ct);

            if (hasChildren)
            {
                return Result.Failure(ErrorType.Conflict, "Cannot delete Education Program that has child programs");
            }

            _dbContext.EducationPrograms.Remove(program);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success("Education Program deleted successfully");
        }
    }
}
