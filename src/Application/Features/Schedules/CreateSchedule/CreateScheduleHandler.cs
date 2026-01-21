using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Schedules.CreateSchedule
{
    public class CreateScheduleHandler : IRequestHandler<CreateScheduleCommand, Result<int>>
    {
        private readonly IDbContext _context;

        public CreateScheduleHandler(IDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(CreateScheduleCommand request, CancellationToken ct)
        {
            // 1. Validate LecturerSubject exists (Lecturer can teach this Subject)
            var lecturerSubject = await _context.LecturerSubjects
                .FirstOrDefaultAsync(ls => ls.LecturerId == request.LecturerId
                                        && ls.SubjectId == request.SubjectId, ct);

            if (lecturerSubject is null)
            {
                return Result.Failure<int>(ErrorType.Validation,
                    $"Lecturer {request.LecturerId} is not assigned to teach Subject {request.SubjectId}.");
            }

            // 2. Validate TimeTable exists
            var timeTableExists = await _context.TimeTables.AnyAsync(t => t.Id == request.TimeTableId, ct);
            if (!timeTableExists)
            {
                return Result.Failure<int>(ErrorType.NotFound,
                    $"TimeTable with ID {request.TimeTableId} not found.");
            }

            // 3. Validate SubjectClassroom exists (Classroom is valid for this Subject and LessonType)
            var subjectClassroomExists = await _context.SubjectClassrooms
                .AnyAsync(sc => sc.SubjectId == request.SubjectId
                             && sc.LessonType == request.LessonType
                             && sc.ClassroomId == request.ClassroomId, ct);

            if (!subjectClassroomExists)
            {
                return Result.Failure<int>(ErrorType.Validation,
                    $"Classroom {request.ClassroomId} is not configured for Subject {request.SubjectId} with LessonType {request.LessonType}.");
            }

            // 4. Validate ScheduleParent if provided
            if (request.ScheduleParentId.HasValue)
            {
                var parentExists = await _context.Schedules.AnyAsync(s => s.Id == request.ScheduleParentId.Value, ct);
                if (!parentExists)
                {
                    return Result.Failure<int>(ErrorType.NotFound,
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
                return Result.Failure<int>(ErrorType.NotFound,
                    $"Groups not found: {string.Join(", ", missingIds)}");
            }

            // 6. Validate all groups have the same SemesterId as provided
            var invalidSemesterGroups = groups.Where(g => g.SemesterId != request.SemesterId).ToList();
            if (invalidSemesterGroups.Any())
            {
                return Result.Failure<int>(ErrorType.Validation,
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
                return Result.Failure<int>(ErrorType.Validation,
                    $"Groups {string.Join(", ", groupsWithoutAssignment)} are not assigned to Lecturer {request.LecturerId} for Subject {request.SubjectId} with LessonType {request.LessonType}.");
            }

            // 8. Check for classroom conflict (same date + timeTableId + classroom + weekType)
            var classroomConflict = await _context.Schedules
                .AnyAsync(s => s.ScheduleDate.Date == request.ScheduleDate.Date
                            && s.TimeTableId == request.TimeTableId
                            && s.ClassroomId == request.ClassroomId
                            && s.WeekType == request.WeekType, ct);

            if (classroomConflict)
            {
                return Result.Failure<int>(ErrorType.Validation,
                    $"Classroom {request.ClassroomId} is already scheduled at the same date, time slot, and week type.");
            }

            // 9. Check for lecturer conflict (same date + timeTableId + lecturer + weekType)
            var lecturerConflict = await _context.Schedules
                .AnyAsync(s => s.ScheduleDate.Date == request.ScheduleDate.Date
                            && s.TimeTableId == request.TimeTableId
                            && s.LecturerId == request.LecturerId
                            && s.WeekType == request.WeekType, ct);

            if (lecturerConflict)
            {
                return Result.Failure<int>(ErrorType.Validation,
                    $"Lecturer {request.LecturerId} is already scheduled at the same date, time slot, and week type.");
            }

            // 10. Check for group conflicts (same date + timeTableId + group + weekType)
            var groupConflicts = await _context.ScheduleGroups
                .Where(sg => request.GroupIds.Contains(sg.GroupId))
                .Where(sg => sg.Schedule.ScheduleDate.Date == request.ScheduleDate.Date
                          && sg.Schedule.TimeTableId == request.TimeTableId
                          && sg.Schedule.WeekType == request.WeekType)
                .Select(sg => sg.GroupId)
                .Distinct()
                .ToListAsync(ct);

            if (groupConflicts.Any())
            {
                return Result.Failure<int>(ErrorType.Validation,
                    $"Groups {string.Join(", ", groupConflicts)} are already scheduled at the same date, time slot, and week type.");
            }

            // Create Schedule with ScheduleGroups (single transaction)
            var schedule = new Schedule
            {
                SubjectId = request.SubjectId,
                LecturerId = request.LecturerId,
                LessonTypeId = request.LessonType,
                ClassroomId = request.ClassroomId,
                TimeTableId = request.TimeTableId,
                WeekType = request.WeekType,
                ScheduleDate = request.ScheduleDate,
                SemesterId = request.SemesterId,
                ScheduleParentId = request.ScheduleParentId,
                ScheduleGroups = request.GroupIds.Select(groupId => new ScheduleGroup
                {
                    GroupId = groupId
                }).ToList()
            };

            await _context.Schedules.AddAsync(schedule, ct);
            await _context.SaveChangesAsync(ct);

            return Result.Success(schedule.Id, "Schedule created successfully.");
        }
    }
}
