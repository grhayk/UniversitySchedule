using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Students.UpdateStudent
{
    internal class UpdateStudentHandler : IRequestHandler<UpdateStudentCommand, Result>
    {
        private readonly IDbContext _context;

        public UpdateStudentHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateStudentCommand request, CancellationToken ct)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == request.Id, ct);

            if (student is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Student with ID {request.Id} not found.");
            }

            // Get group with its education program and semester for derived fields
            var group = await _context.Groups
                .Include(g => g.EducationProgram)
                .Include(g => g.Semester)
                .FirstOrDefaultAsync(g => g.Id == request.GroupId, ct);

            if (group is null)
            {
                return Result.Failure(ErrorType.NotFound, $"Group with ID {request.GroupId} not found.");
            }

            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.BirthDate = request.BirthDate;
            student.GroupId = request.GroupId;
            // Update derived fields
            student.StructureId = group.EducationProgram.StructureId;
            student.EducationDegree = group.Semester.EducationDegree;
            student.EducationType = group.Semester.EducationType;

            await _context.SaveChangesAsync(ct);

            return Result.Success("Student updated successfully");
        }
    }
}
