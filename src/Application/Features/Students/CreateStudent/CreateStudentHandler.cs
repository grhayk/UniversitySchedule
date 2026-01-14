using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Students.CreateStudent
{
    internal class CreateStudentHandler : IRequestHandler<CreateStudentCommand, Result<int>>
    {
        private readonly IDbContext _context;

        public CreateStudentHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(CreateStudentCommand request, CancellationToken ct)
        {
            // Get group with its education program and semester
            var group = await _context.Groups
                .Include(g => g.EducationProgram)
                .Include(g => g.Semester)
                .FirstOrDefaultAsync(g => g.Id == request.GroupId, ct);

            if (group is null)
            {
                return Result.Failure<int>(ErrorType.NotFound, $"Group with ID {request.GroupId} not found.");
            }

            // Group must be a parent group (lecture group)
            if (group.ParentId != null)
            {
                return Result.Failure<int>(ErrorType.Validation, "Student can only be assigned to a parent group (lecture group).");
            }

            var student = new Student
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                GroupId = request.GroupId,
                // Derived from Group's relationships
                StructureId = group.EducationProgram.StructureId,
                EducationDegree = group.Semester.EducationDegree,
                EducationType = group.Semester.EducationType
            };

            _context.Students.Add(student);

            // Also create StudentGroup record for the parent group
            var studentGroup = new StudentGroup
            {
                Student = student,
                GroupId = request.GroupId,
                SemesterId = group.SemesterId
            };

            _context.StudentGroups.Add(studentGroup);
            await _context.SaveChangesAsync(ct);

            return Result.Success(student.Id, "Student created successfully");
        }
    }
}
