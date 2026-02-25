using Application.Core;
using Application.Interfaces;
using Application.Models.ScheduleGeneration;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
            var totalSw = Stopwatch.StartNew();
            var stepSw = Stopwatch.StartNew();

            // Step 1: Load all necessary data
            Console.WriteLine("[Step 1] Loading data from DB...");

            var gswls = await _context.GroupSubjectsWithLecturer
                .Include(g => g.LecturerSubject).ThenInclude(ls => ls.Subject).ThenInclude(s => s.SubjectConfigs)
                .Include(g => g.Group).ThenInclude(g => g!.Children)
                .Include(g => g.Group).ThenInclude(g => g!.Parent)
                .Include(g => g.Group).ThenInclude(g => g!.EducationProgram)
                .Include(g => g.Flow).ThenInclude(f => f!.FlowGroups).ThenInclude(fg => fg.Group).ThenInclude(g => g.Children)
                .Include(g => g.Flow).ThenInclude(f => f!.FlowGroups).ThenInclude(fg => fg.Group).ThenInclude(g => g.EducationProgram)
                .Where(g =>
                    (g.GroupId.HasValue && g.Group!.IsActive) ||
                    (g.FlowId.HasValue && g.Flow!.IsActive))
                .AsNoTracking()
                .ToListAsync(ct);

            Console.WriteLine($"  Loaded {gswls.Count} GSWL records ({stepSw.ElapsedMilliseconds}ms)");

            if (!gswls.Any())
            {
                return Result.Failure<GenerateScheduleResult>(ErrorType.Validation,
                    "No active GroupSubjectWithLecturer records found.");
            }

            stepSw.Restart();
            var subjectClassrooms = await _context.SubjectClassrooms.AsNoTracking().ToListAsync(ct);
            Console.WriteLine($"  Loaded {subjectClassrooms.Count} SubjectClassroom records ({stepSw.ElapsedMilliseconds}ms)");

            stepSw.Restart();
            var classrooms = await _context.Classrooms
                .Include(c => c.Characteristics)
                .Include(c => c.Structure)
                .AsNoTracking()
                .ToListAsync(ct);
            Console.WriteLine($"  Loaded {classrooms.Count} Classrooms ({stepSw.ElapsedMilliseconds}ms)");

            var timeTables = await _context.TimeTables
                .OrderBy(t => t.StartTime)
                .AsNoTracking()
                .ToListAsync(ct);

            if (timeTables.Count < 6)
            {
                return Result.Failure<GenerateScheduleResult>(ErrorType.Validation,
                    $"Expected at least 6 TimeTables, found {timeTables.Count}.");
            }

            var classroomCapacity = classrooms
                .Where(c => c.Characteristics != null)
                .ToDictionary(c => c.Id, c => c.Characteristics!.SeatCapacity);

            stepSw.Restart();
            var studentCountByGroup = await _context.Students
                .Where(s => s.GroupId.HasValue)
                .GroupBy(s => s.GroupId!.Value)
                .Select(g => new { GroupId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.GroupId, x => x.Count, ct);
            Console.WriteLine($"  Loaded student counts for {studentCountByGroup.Count} groups ({stepSw.ElapsedMilliseconds}ms)");
            Console.WriteLine($"[Step 1] Done ({totalSw.ElapsedMilliseconds}ms total)");

            // Step 2: Build LessonDemand list
            Console.WriteLine("[Step 2] Building demands...");
            stepSw.Restart();

            var demands = new List<LessonDemand>();
            var errors = new List<string>();

            foreach (var gswl in gswls)
            {
                var lecturerId = gswl.LecturerSubject.LecturerId;
                var subjectId = gswl.LecturerSubject.SubjectId;

                var subjectConfig = gswl.LecturerSubject.Subject.SubjectConfigs
                    .FirstOrDefault(sc => sc.LessonType == gswl.LessonType);

                if (subjectConfig is null || subjectConfig.Hours == 0)
                    continue;

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
                        busyGroupIds.Add(group.Id);
                        foreach (var child in group.Children)
                            busyGroupIds.Add(child.Id);
                    }
                    else
                    {
                        busyGroupIds.Add(group.Id);
                        if (group.ParentId.HasValue)
                            busyGroupIds.Add(group.ParentId.Value);
                    }

                    if (gswl.LessonType == LessonType.Lecture && group.Children.Any())
                    {
                        studentCount = group.Children
                            .Sum(c => studentCountByGroup.GetValueOrDefault(c.Id, 0));
                        studentCount += studentCountByGroup.GetValueOrDefault(group.Id, 0);
                    }
                    else
                    {
                        studentCount = studentCountByGroup.GetValueOrDefault(group.Id, 0);
                    }
                }
                else
                {
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

                // Valid classrooms selection
                var scEntries = subjectClassrooms
                    .Where(sc => sc.SubjectId == subjectId && sc.LessonType == gswl.LessonType)
                    .Select(sc => sc.ClassroomId)
                    .Distinct()
                    .ToList();

                List<int> validClassroomIds;
                List<int> preferredClassroomIds = new();

                if (scEntries.Count > 0)
                {
                    var scValid = scEntries
                        .Where(cId => classroomCapacity.GetValueOrDefault(cId, 0) >= studentCount)
                        .ToList();

                    // SubjectClassroom entries become preferred; also include all
                    // university-level classrooms with sufficient capacity as fallback.
                    // This prevents infeasibility when too many events share too few
                    // SubjectClassroom entries (e.g., 124 events for 2 classrooms = 120 max slots).
                    var universityFallback = classrooms
                        .Where(c => c.Structure.ParentId == null
                                    && classroomCapacity.GetValueOrDefault(c.Id, 0) >= studentCount)
                        .Select(c => c.Id)
                        .ToList();

                    preferredClassroomIds = scValid;
                    validClassroomIds = universityFallback.Union(scValid).ToList();
                }
                else
                {
                    byte? groupBuilding = null;
                    int? chairId = null;

                    if (gswl.GroupId.HasValue)
                    {
                        var group = gswl.Group!;
                        chairId = group.ParentId.HasValue && group.Parent?.EducationProgram != null
                            ? group.Parent.EducationProgram.StructureId
                            : group.EducationProgram?.StructureId;
                    }
                    else if (gswl.FlowId.HasValue)
                    {
                        chairId = gswl.Flow!.FlowGroups
                            .Select(fg => fg.Group.EducationProgram?.StructureId)
                            .FirstOrDefault(id => id.HasValue);
                    }

                    if (chairId.HasValue)
                    {
                        groupBuilding = classrooms
                            .FirstOrDefault(c => c.StructureId == chairId.Value
                                                 && c.Characteristics?.Type == ClassroomType.ChairRoom)
                            ?.Building;
                    }

                    var sameBuildingIds = groupBuilding.HasValue
                        ? classrooms
                            .Where(c => c.Building == groupBuilding.Value
                                        && c.Structure.ParentId == null
                                        && classroomCapacity.GetValueOrDefault(c.Id, 0) >= studentCount)
                            .Select(c => c.Id)
                            .ToList()
                        : new List<int>();

                    var universityClassroomIds = classrooms
                        .Where(c => c.Structure.ParentId == null
                                    && classroomCapacity.GetValueOrDefault(c.Id, 0) >= studentCount)
                        .Select(c => c.Id)
                        .ToList();

                    validClassroomIds = universityClassroomIds;
                    preferredClassroomIds = sameBuildingIds;
                }

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
                    PreferredClassroomIds = preferredClassroomIds,
                    StudentCount = studentCount,
                    ScheduleGroupIds = scheduleGroupIds,
                    SemesterId = semesterId
                });
            }

            Console.WriteLine($"  Built {demands.Count} demands, {errors.Count} errors ({stepSw.ElapsedMilliseconds}ms)");

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
            Console.WriteLine("[Step 3] Expanding demands into events...");
            stepSw.Restart();

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

            Console.WriteLine($"  {events.Count} events from {demands.Count} demands ({stepSw.ElapsedMilliseconds}ms)");

            // Step 4: Call solver
            Console.WriteLine("[Step 4] Calling CP-SAT solver...");
            stepSw.Restart();

            var input = new ScheduleGeneratorInput
            {
                Demands = demands,
                Events = events,
                AllClassroomIds = demands.SelectMany(d => d.ValidClassroomIds).Distinct().ToList(),
                TimeLimitSeconds = 60
            };

            var output = _generator.Generate(input);

            Console.WriteLine($"  Solver finished: {output.SolverStatus} ({stepSw.ElapsedMilliseconds}ms)");

            if (!output.IsFeasible)
            {
                return Result.Failure<GenerateScheduleResult>(ErrorType.Validation,
                    $"Solver could not find a feasible schedule. Status: {output.SolverStatus}. " +
                    $"Total demands: {demands.Count}, Total events: {events.Count}.");
            }

            // Step 5: Map output to Schedule entities
            Console.WriteLine("[Step 5] Mapping solver output to Schedule entities...");
            stepSw.Restart();

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

            Console.WriteLine($"  Created {schedulesToCreate.Count} schedule entities ({stepSw.ElapsedMilliseconds}ms)");

            // Step 6: Save all in single transaction
            Console.WriteLine("[Step 6] Saving to database...");
            stepSw.Restart();

            await _context.Schedules.AddRangeAsync(schedulesToCreate, ct);
            await _context.SaveChangesAsync(ct);

            Console.WriteLine($"  Saved ({stepSw.ElapsedMilliseconds}ms)");
            Console.WriteLine($"[DONE] Total time: {totalSw.Elapsed.TotalSeconds:F2}s");

            return Result.Success(new GenerateScheduleResult
            {
                TotalSchedulesCreated = schedulesToCreate.Count,
                TotalDemandsProcessed = demands.Count,
                SolverStatus = output.SolverStatus!
            }, $"Schedule generated successfully. {schedulesToCreate.Count} entries created.");
        }
    }
}
