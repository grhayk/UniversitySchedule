using Application.Core;
using Application.Interfaces;
using Application.Models.ScheduleGeneration;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Schedules.GenerateSchedule
{
    public class GenerateScheduleHandler : IRequestHandler<GenerateScheduleCommand, Result<GenerateScheduleResult>>
    {
        private readonly IDbContext _context;
        private readonly IScheduleGenerator _generator;

        public GenerateScheduleHandler(IDbContext context, IScheduleGenerator generator)
        {
            _context = context;
            _generator = generator;
        }

        public async Task<Result<GenerateScheduleResult>> Handle(GenerateScheduleCommand request, CancellationToken ct)
        {
            // Step 1: Load all necessary data

            // All active GroupSubjectWithLecturer with related data
            var gswls = await _context.GroupSubjectsWithLecturer
                .Include(g => g.LecturerSubject).ThenInclude(ls => ls.Subject).ThenInclude(s => s.SubjectConfigs)
                .Include(g => g.Group).ThenInclude(g => g!.Children)
                .Include(g => g.Group).ThenInclude(g => g!.Parent)
                .Include(g => g.Flow).ThenInclude(f => f!.FlowGroups).ThenInclude(fg => fg.Group).ThenInclude(g => g.Children)
                .Where(g =>
                    (g.GroupId.HasValue && g.Group!.IsActive) ||
                    (g.FlowId.HasValue && g.Flow!.IsActive))
                .AsNoTracking()
                .ToListAsync(ct);

            if (!gswls.Any())
            {
                return Result.Failure<GenerateScheduleResult>(ErrorType.Validation,
                    "No active GroupSubjectWithLecturer records found.");
            }

            var subjectClassrooms = await _context.SubjectClassrooms.AsNoTracking().ToListAsync(ct);

            var classrooms = await _context.Classrooms
                .Include(c => c.Characteristics)
                .AsNoTracking()
                .ToListAsync(ct);

            var timeTables = await _context.TimeTables
                .OrderBy(t => t.StartTime)
                .AsNoTracking()
                .ToListAsync(ct);

            if (timeTables.Count < 6)
            {
                return Result.Failure<GenerateScheduleResult>(ErrorType.Validation,
                    $"Expected at least 6 TimeTables, found {timeTables.Count}.");
            }

            // Build classroom capacity lookup
            var classroomCapacity = classrooms
                .Where(c => c.Characteristics != null)
                .ToDictionary(c => c.Id, c => c.Characteristics!.SeatCapacity);

            // Build student count per parent group (sum students from group's Students)
            var studentCountByGroup = await _context.Students
                .Where(s => s.GroupId.HasValue)
                .GroupBy(s => s.GroupId!.Value)
                .Select(g => new { GroupId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.GroupId, x => x.Count, ct);

            // Step 2: Build LessonDemand list
            var demands = new List<LessonDemand>();
            var errors = new List<string>();

            foreach (var gswl in gswls)
            {
                var lecturerId = gswl.LecturerSubject.LecturerId;
                var subjectId = gswl.LecturerSubject.SubjectId;

                // Get hours from SubjectConfig
                var subjectConfig = gswl.LecturerSubject.Subject.SubjectConfigs
                    .FirstOrDefault(sc => sc.LessonType == gswl.LessonType);

                if (subjectConfig is null || subjectConfig.Hours == 0)
                    continue; // No hours configured for this lesson type, skip

                var busyGroupIds = new HashSet<int>();
                var scheduleGroupIds = new List<int>();
                int studentCount;
                int semesterId;

                if (gswl.GroupId.HasValue)
                {
                    var group = gswl.Group!;
                    scheduleGroupIds.Add(group.Id);
                    semesterId = group.SemesterId;

                    if (gswl.LessonType == LessonType.Lecture)
                    {
                        // Parent group lecture: parent + all children are busy
                        busyGroupIds.Add(group.Id);
                        foreach (var child in group.Children)
                            busyGroupIds.Add(child.Id);
                    }
                    else
                    {
                        // Child group practical/lab: child + parent are busy
                        busyGroupIds.Add(group.Id);
                        if (group.ParentId.HasValue)
                            busyGroupIds.Add(group.ParentId.Value);
                    }

                    // Student count: for lectures sum children, for practicals/labs use child's count
                    if (gswl.LessonType == LessonType.Lecture && group.Children.Any())
                    {
                        studentCount = group.Children
                            .Sum(c => studentCountByGroup.GetValueOrDefault(c.Id, 0));
                        // Also include students directly on parent group
                        studentCount += studentCountByGroup.GetValueOrDefault(group.Id, 0);
                    }
                    else
                    {
                        studentCount = studentCountByGroup.GetValueOrDefault(group.Id, 0);
                    }
                }
                else
                {
                    // Flow
                    var flow = gswl.Flow!;
                    studentCount = flow.StudentsCount;
                    semesterId = flow.SemesterId;

                    foreach (var fg in flow.FlowGroups)
                    {
                        scheduleGroupIds.Add(fg.GroupId);
                        busyGroupIds.Add(fg.GroupId);
                        foreach (var child in fg.Group.Children)
                            busyGroupIds.Add(child.Id);
                    }
                }

                // Valid classrooms: SubjectClassroom filtered by capacity
                var validClassroomIds = subjectClassrooms
                    .Where(sc => sc.SubjectId == subjectId && sc.LessonType == gswl.LessonType)
                    .Select(sc => sc.ClassroomId)
                    .Where(cId => classroomCapacity.GetValueOrDefault(cId, 0) >= studentCount)
                    .Distinct()
                    .ToList();

                if (validClassroomIds.Count == 0)
                {
                    errors.Add($"No valid classrooms for Subject {subjectId} ({gswl.LecturerSubject.Subject.Name}), " +
                               $"LessonType {gswl.LessonType}, StudentCount {studentCount}.");
                    continue;
                }

                demands.Add(new LessonDemand
                {
                    GroupSubjectWithLecturerId = gswl.Id,
                    LecturerId = lecturerId,
                    SubjectId = subjectId,
                    LessonType = gswl.LessonType,
                    Hours = subjectConfig.Hours,
                    BusyGroupIds = busyGroupIds.ToList(),
                    ValidClassroomIds = validClassroomIds,
                    StudentCount = studentCount,
                    ScheduleGroupIds = scheduleGroupIds,
                    SemesterId = semesterId
                });
            }

            if (errors.Any())
            {
                return Result.Failure<GenerateScheduleResult>(ErrorType.Validation,
                    $"Cannot generate schedule. Issues found:\n{string.Join("\n", errors)}");
            }

            if (!demands.Any())
            {
                return Result.Failure<GenerateScheduleResult>(ErrorType.Validation,
                    "No valid demands to schedule after processing.");
            }

            // Step 3: Expand demands into LessonEvents
            var events = new List<LessonEvent>();
            for (int d = 0; d < demands.Count; d++)
            {
                for (int s = 0; s < demands[d].Hours; s++)
                {
                    events.Add(new LessonEvent
                    {
                        EventIndex = events.Count,
                        DemandIndex = d
                    });
                }
            }

            // Step 4: Call solver
            var input = new ScheduleGeneratorInput
            {
                Demands = demands,
                Events = events,
                AllClassroomIds = demands.SelectMany(d => d.ValidClassroomIds).Distinct().ToList(),
                TimeLimitSeconds = 60
            };

            var output = _generator.Generate(input);

            if (!output.IsFeasible)
            {
                return Result.Failure<GenerateScheduleResult>(ErrorType.Validation,
                    $"Solver could not find a feasible schedule. Status: {output.SolverStatus}. " +
                    $"Total demands: {demands.Count}, Total events: {events.Count}.");
            }

            // Step 5: Map output to Schedule entities
            var timeTableIds = timeTables.Select(t => t.Id).ToList();
            var schedulesToCreate = new List<Schedule>();

            foreach (var placed in output.PlacedLessons)
            {
                var demand = demands[placed.DemandIndex];
                int weekIndex = placed.Timeslot / input.TimeslotsPerWeek;
                int remainder = placed.Timeslot % input.TimeslotsPerWeek;
                int dayIndex = remainder / input.SlotsPerDay;
                int slotIndex = remainder % input.SlotsPerDay;

                var weekType = weekIndex == 0 ? WeekType.Numerator : WeekType.Denominator;
                var timeTableId = timeTableIds[slotIndex];
                var scheduleDate = request.StartDate.AddDays(weekIndex * 7 + dayIndex);

                var schedule = new Schedule
                {
                    SubjectId = demand.SubjectId,
                    LecturerId = demand.LecturerId,
                    LessonTypeId = demand.LessonType,
                    ClassroomId = placed.ClassroomId,
                    TimeTableId = timeTableId,
                    WeekType = weekType,
                    ScheduleDate = scheduleDate,
                    SemesterId = demand.SemesterId,
                    ScheduleParentId = null,
                    ScheduleGroups = demand.ScheduleGroupIds
                        .Select(gId => new ScheduleGroup { GroupId = gId })
                        .ToList()
                };

                schedulesToCreate.Add(schedule);
            }

            // Step 6: Save all in single transaction
            await _context.Schedules.AddRangeAsync(schedulesToCreate, ct);
            await _context.SaveChangesAsync(ct);

            return Result.Success(new GenerateScheduleResult
            {
                TotalSchedulesCreated = schedulesToCreate.Count,
                TotalDemandsProcessed = demands.Count,
                SolverStatus = output.SolverStatus!
            }, $"Schedule generated successfully. {schedulesToCreate.Count} entries created.");
        }
    }
}
