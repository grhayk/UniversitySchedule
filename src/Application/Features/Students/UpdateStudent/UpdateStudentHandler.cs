using Application.Core;
using Application.Interfaces;
using Domain.Entities;
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

            // Group must be a parent group (lecture group)
            if (group.ParentId != null)
            {
                return Result.Failure(ErrorType.Validation, "Student can only be assigned to a parent group (lecture group).");
            }

            var oldGroupId = student.GroupId;

            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.BirthDate = request.BirthDate;
            student.GroupId = request.GroupId;
            // Update derived fields
            student.StructureId = group.EducationProgram.StructureId;
            student.EducationDegree = group.Semester.EducationDegree;
            student.EducationType = group.Semester.EducationType;

            // If group changed, update StudentGroup records
            if (oldGroupId != request.GroupId)
            {
                // Remove all existing StudentGroup records for this student
                var existingStudentGroups = await _context.StudentGroups
                    .Where(sg => sg.StudentId == request.Id)
                    .ToListAsync(ct);

                _context.StudentGroups.RemoveRange(existingStudentGroups);

                // Create new StudentGroup for the new parent group
                var studentGroup = new StudentGroup
                {
                    StudentId = request.Id,
                    GroupId = request.GroupId,
                    SemesterId = group.SemesterId
                };

                _context.StudentGroups.Add(studentGroup);
            }

            await _context.SaveChangesAsync(ct);

            return Result.Success("Student updated successfully");
        }
    }
}
