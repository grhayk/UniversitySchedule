using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Schedules.UpdateSchedule
{
    public class UpdateScheduleHandler : IRequestHandler<UpdateScheduleCommand, Result>
    {
        private readonly IDbContext _context;

        public UpdateScheduleHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateScheduleCommand request, CancellationToken ct)
        {
            // Get existing schedule
            var schedule = await _context.Schedules
                .Include(s => s.ScheduleGroups)
                .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

            if (schedule is null)
            {
                return Result.Failure(ErrorType.NotFound,
                    $"Schedule with ID {request.Id} not found.");
            }

            // 1. Validate LecturerSubject exists
            var lecturerSubject = await _context.LecturerSubjects
                .FirstOrDefaultAsync(ls => ls.LecturerId == request.LecturerId
                                        && ls.SubjectId == request.SubjectId, ct);

            if (lecturerSubject is null)
            {
                return Result.Failure(ErrorType.Validation,
                    $"Lecturer {request.LecturerId} is not assigned to teach Subject {request.SubjectId}.");
            }

            // 2. Validate TimeTable exists
            var timeTableExists = await _context.TimeTables.AnyAsync(t => t.Id == request.TimeTableId, ct);
            if (!timeTableExists)
            {
                return Result.Failure(ErrorType.NotFound,
                    $"TimeTable with ID {request.TimeTableId} not found.");
            }

            // 3. Validate SubjectClassroom exists
            var subjectClassroomExists = await _context.SubjectClassrooms
                .AnyAsync(sc => sc.SubjectId == request.SubjectId
                             && sc.LessonType == request.LessonType
                             && sc.ClassroomId == request.ClassroomId, ct);

            if (!subjectClassroomExists)
            {
                return Result.Failure(ErrorType.Validation,
                    $"Classroom {request.ClassroomId} is not configured for Subject {request.SubjectId} with LessonType {request.LessonType}.");
            }

            // 4. Validate ScheduleParent if provided
            if (request.ScheduleParentId.HasValue)
            {
                if (request.ScheduleParentId.Value == request.Id)
                {
                    return Result.Failure(ErrorType.Validation,
                        "Schedule cannot be its own parent.");
                }

                var parentExists = await _context.Schedules.AnyAsync(s => s.Id == request.ScheduleParentId.Value, ct);
                if (!parentExists)
                {
                    return Result.Failure(ErrorType.NotFound,
                        $"Parent Schedule with ID {request.ScheduleParentId} not found.");
                }
            }

            // 5. Get and validate Groups
            var groups = await _context.Groups
                .Where(g => request.GroupIds.Contains(g.Id))
                .ToListAsync(ct);

            if (groups.Count != request.GroupIds.Count)
            {
                var foundIds = groups.Select(g => g.Id).ToHashSet();
                var missingIds = request.GroupIds.Where(id => !foundIds.Contains(id));
                return Result.Failure(ErrorType.NotFound,
                    $"Groups not found: {string.Join(", ", missingIds)}");
            }

            // 6. Validate all groups have the same SemesterId
            var invalidSemesterGroups = groups.Where(g => g.SemesterId != request.SemesterId).ToList();
            if (invalidSemesterGroups.Any())
            {
                return Result.Failure(ErrorType.Validation,
                    $"Groups {string.Join(", ", invalidSemesterGroups.Select(g => g.Id))} do not belong to Semester {request.SemesterId}.");
            }

            // 7. Validate GroupSubjectWithLecturer exists for each group
            var groupSubjectsWithLecturer = await _context.GroupSubjectsWithLecturer
                .Where(gsl => gsl.LecturerSubjectId == lecturerSubject.Id
                           && gsl.LessonType == request.LessonType
                           && request.GroupIds.Contains(gsl.GroupId))
                .Select(gsl => gsl.GroupId)
                .ToListAsync(ct);

            var groupsWithoutAssignment = request.GroupIds.Except(groupSubjectsWithLecturer).ToList();
            if (groupsWithoutAssignment.Any())
            {
                return Result.Failure(ErrorType.Validation,
                    $"Groups {string.Join(", ", groupsWithoutAssignment)} are not assigned to Lecturer {request.LecturerId} for Subject {request.SubjectId} with LessonType {request.LessonType}.");
            }

            // 8. Check for classroom conflict (excluding current schedule)
            var classroomConflict = await _context.Schedules
                .AnyAsync(s => s.Id != request.Id
                            && s.ScheduleDate.Date == request.ScheduleDate.Date
                            && s.TimeTableId == request.TimeTableId
                            && s.ClassroomId == request.ClassroomId
                            && s.WeekType == request.WeekType, ct);

            if (classroomConflict)
            {
                return Result.Failure(ErrorType.Validation,
                    $"Classroom {request.ClassroomId} is already scheduled at the same date, time slot, and week type.");
            }

            // 9. Check for lecturer conflict (excluding current schedule)
            var lecturerConflict = await _context.Schedules
                .AnyAsync(s => s.Id != request.Id
                            && s.ScheduleDate.Date == request.ScheduleDate.Date
                            && s.TimeTableId == request.TimeTableId
                            && s.LecturerId == request.LecturerId
                            && s.WeekType == request.WeekType, ct);

            if (lecturerConflict)
            {
                return Result.Failure(ErrorType.Validation,
                    $"Lecturer {request.LecturerId} is already scheduled at the same date, time slot, and week type.");
            }

            // 10. Check for group conflicts (excluding current schedule's groups)
            var groupConflicts = await _context.ScheduleGroups
                .Where(sg => sg.ScheduleId != request.Id)
                .Where(sg => request.GroupIds.Contains(sg.GroupId))
                .Where(sg => sg.Schedule.ScheduleDate.Date == request.ScheduleDate.Date
                          && sg.Schedule.TimeTableId == request.TimeTableId
                          && sg.Schedule.WeekType == request.WeekType)
                .Select(sg => sg.GroupId)
                .Distinct()
                .ToListAsync(ct);

            if (groupConflicts.Any())
            {
                return Result.Failure(ErrorType.Validation,
                    $"Groups {string.Join(", ", groupConflicts)} are already scheduled at the same date, time slot, and week type.");
            }

            // Update Schedule
            schedule.SubjectId = request.SubjectId;
            schedule.LecturerId = request.LecturerId;
            schedule.LessonTypeId = request.LessonType;
            schedule.ClassroomId = request.ClassroomId;
            schedule.TimeTableId = request.TimeTableId;
            schedule.WeekType = request.WeekType;
            schedule.ScheduleDate = request.ScheduleDate;
            schedule.SemesterId = request.SemesterId;
            schedule.ScheduleParentId = request.ScheduleParentId;

            // Update ScheduleGroups - clear and add new (single transaction)
            schedule.ScheduleGroups.Clear();
            foreach (var groupId in request.GroupIds)
            {
                schedule.ScheduleGroups.Add(new ScheduleGroup { GroupId = groupId });
            }

            await _context.SaveChangesAsync(ct);

            return Result.Success("Schedule updated successfully.");
        }
    }
}
