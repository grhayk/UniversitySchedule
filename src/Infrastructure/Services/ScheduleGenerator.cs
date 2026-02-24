using Application.Interfaces;
using Application.Models.ScheduleGeneration;
using Google.OrTools.Sat;
using System.Diagnostics;

namespace Infrastructure.Services
{
    public class ScheduleGenerator : IScheduleGenerator
    {
        public ScheduleGeneratorOutput Generate(ScheduleGeneratorInput input)
        {
            var sw = Stopwatch.StartNew();
            var demands = input.Demands;
            var events = input.Events;
            int totalTimeslots = input.TotalTimeslots;
            int slotsPerDay = input.SlotsPerDay;
            int timeslotsPerWeek = input.TimeslotsPerWeek;

            Console.WriteLine($"  Solver input: {events.Count} events, {demands.Count} demands, {input.AllClassroomIds.Count} classrooms, {totalTimeslots} timeslots");

            if (events.Count == 0)
            {
                return new ScheduleGeneratorOutput
                {
                    IsFeasible = true,
                    SolverStatus = "NoEvents"
                };
            }

            // =================================================================
            // PHASE 1: Assign timeslots using CP-SAT (no classroom variables)
            // =================================================================
            Console.WriteLine($"  [Phase 1] Building timeslot-only model...");

            var model = new CpModel();
            var timeslotVars = new IntVar[events.Count];

            for (int e = 0; e < events.Count; e++)
                timeslotVars[e] = model.NewIntVar(0, totalTimeslots - 1, $"ts_{e}");

            Console.WriteLine($"    Variables: {events.Count} ({sw.ElapsedMilliseconds}ms)");

            // HARD: Lecturer no-overlap
            var eventsByLecturer = events
                .Select((ev, i) => (ev, i))
                .GroupBy(x => demands[x.ev.DemandIndex].LecturerId);

            int lecConstraints = 0;
            foreach (var group in eventsByLecturer)
            {
                var indices = group.Select(x => x.i).ToArray();
                if (indices.Length > 1)
                {
                    model.AddAllDifferent(indices.Select(i => timeslotVars[i]).ToArray());
                    lecConstraints++;
                }
            }
            Console.WriteLine($"    Lecturer constraints: {lecConstraints} ({sw.ElapsedMilliseconds}ms)");

            // HARD: Group no-overlap
            var eventsByBusyGroup = new Dictionary<int, List<int>>();
            for (int e = 0; e < events.Count; e++)
            {
                var demand = demands[events[e].DemandIndex];
                foreach (var gId in demand.BusyGroupIds)
                {
                    if (!eventsByBusyGroup.TryGetValue(gId, out var list))
                    {
                        list = new List<int>();
                        eventsByBusyGroup[gId] = list;
                    }
                    list.Add(e);
                }
            }

            int grpConstraints = 0;
            foreach (var (_, eventIndices) in eventsByBusyGroup)
            {
                if (eventIndices.Count > 1)
                {
                    model.AddAllDifferent(eventIndices.Select(i => timeslotVars[i]).ToArray());
                    grpConstraints++;
                }
            }
            Console.WriteLine($"    Group constraints: {grpConstraints} ({sw.ElapsedMilliseconds}ms)");

            // HARD: Hours distribution per demand
            var isInWeek0 = new BoolVar[events.Count];
            for (int e = 0; e < events.Count; e++)
            {
                isInWeek0[e] = model.NewBoolVar($"w0_{e}");
                model.Add(timeslotVars[e] < timeslotsPerWeek).OnlyEnforceIf(isInWeek0[e]);
                model.Add(timeslotVars[e] >= timeslotsPerWeek).OnlyEnforceIf(isInWeek0[e].Not());
            }

            var eventsByDemand = events
                .Select((ev, i) => (ev, i))
                .GroupBy(x => x.ev.DemandIndex);

            foreach (var grp in eventsByDemand)
            {
                var demand = demands[grp.Key];
                var evIndices = grp.Select(x => x.i).ToArray();
                int H = demand.Hours;
                int hi = (H + 1) / 2;
                int lo = H / 2;

                var week0Count = LinearExpr.Sum(evIndices.Select(i => isInWeek0[i]));

                if (hi == lo)
                    model.Add(week0Count == hi);
                else
                {
                    var pickHi = model.NewBoolVar($"pickHi_{grp.Key}");
                    model.Add(week0Count == hi).OnlyEnforceIf(pickHi);
                    model.Add(week0Count == lo).OnlyEnforceIf(pickHi.Not());
                }
            }
            Console.WriteLine($"    Hours distribution done ({sw.ElapsedMilliseconds}ms)");

            // NOTE: Timeslot capacity handled by Phase 1.5 greedy repair (not in CP-SAT model).
            // NOTE: No-same-day and late-slot penalties removed for performance.

            Console.WriteLine($"    Model complete ({sw.ElapsedMilliseconds}ms). Solving...");

            var solver = new CpSolver();
            solver.StringParameters = string.Join(";",
                $"max_time_in_seconds:{input.TimeLimitSeconds}",
                "num_workers:8",
                "log_search_progress:true"
            );

            var callback = new SolverProgressCallback();
            var status = solver.Solve(model, callback);

            Console.WriteLine($"    Solver: {status}, solutions: {callback.SolutionCount}, time: {solver.WallTime():F2}s");

            if (status != CpSolverStatus.Optimal && status != CpSolverStatus.Feasible)
            {
                return new ScheduleGeneratorOutput
                {
                    IsFeasible = false,
                    SolverStatus = $"Phase1_{status}"
                };
            }

            // Extract timeslot assignments
            var timeslotAssignments = new int[events.Count];
            for (int e = 0; e < events.Count; e++)
                timeslotAssignments[e] = (int)solver.Value(timeslotVars[e]);

            Console.WriteLine($"  [Phase 1] Done ({sw.ElapsedMilliseconds}ms)");

            // =================================================================
            // PHASE 1.5: Repair over-capacity timeslots
            // Move excess events from crowded timeslots to ones with room,
            // while respecting lecturer and group AllDifferent constraints.
            // =================================================================
            // Use 65% of classroom count as capacity limit to leave headroom
            // for classroom assignment (not all classrooms are valid for all events)
            int maxPerSlot = (int)(input.AllClassroomIds.Count * 0.65);
            var slotCounts = new int[totalTimeslots];
            for (int e = 0; e < events.Count; e++)
                slotCounts[timeslotAssignments[e]]++;

            int overCapacity = slotCounts.Count(c => c > maxPerSlot);
            if (overCapacity > 0)
            {
                Console.WriteLine($"  [Phase 1.5] Repairing {overCapacity} over-capacity timeslots (max {slotCounts.Max()}, limit {maxPerSlot})...");

                // Build conflict sets: for each lecturer/group, which timeslots are occupied
                var lecturerSlots = new Dictionary<int, HashSet<int>>();
                var groupSlots = new Dictionary<int, HashSet<int>>();

                for (int e = 0; e < events.Count; e++)
                {
                    var demand = demands[events[e].DemandIndex];
                    int ts = timeslotAssignments[e];

                    if (!lecturerSlots.TryGetValue(demand.LecturerId, out var lSet))
                        lecturerSlots[demand.LecturerId] = lSet = new HashSet<int>();
                    lSet.Add(ts);

                    foreach (var gId in demand.BusyGroupIds)
                    {
                        if (!groupSlots.TryGetValue(gId, out var gSet))
                            groupSlots[gId] = gSet = new HashSet<int>();
                        gSet.Add(ts);
                    }
                }

                // Build reverse index: timeslot -> list of events
                var slotEvents = new Dictionary<int, List<int>>();
                for (int e = 0; e < events.Count; e++)
                {
                    int ts = timeslotAssignments[e];
                    if (!slotEvents.TryGetValue(ts, out var list))
                        slotEvents[ts] = list = new List<int>();
                    list.Add(e);
                }

                int totalMoved = 0;

                // Multiple passes — after moving events, new targets may become available
                for (int pass = 0; pass < 10; pass++)
                {
                    int movedThisPass = 0;

                    var overSlots = Enumerable.Range(0, totalTimeslots)
                        .Where(t => slotCounts[t] > maxPerSlot)
                        .OrderByDescending(t => slotCounts[t])
                        .ToList();

                    if (overSlots.Count == 0) break;

                    foreach (int t in overSlots)
                    {
                        if (slotCounts[t] <= maxPerSlot) continue;

                        // Sort events by flexibility (most classroom options first = easiest to place elsewhere)
                        var eventsHere = slotEvents.GetValueOrDefault(t, new List<int>())
                            .Where(e => timeslotAssignments[e] == t) // still here
                            .OrderByDescending(e => demands[events[e].DemandIndex].ValidClassroomIds.Count)
                            .ToList();

                        foreach (int e in eventsHere)
                        {
                            if (slotCounts[t] <= maxPerSlot) break;

                            var dem = demands[events[e].DemandIndex];
                            bool inWeek0 = timeslotAssignments[e] < timeslotsPerWeek;

                            int bestSlot = -1;
                            int bestCount = int.MaxValue;
                            bool bestSameWeek = false;

                            for (int nt = 0; nt < totalTimeslots; nt++)
                            {
                                if (slotCounts[nt] >= maxPerSlot) continue;

                                if (lecturerSlots.TryGetValue(dem.LecturerId, out var lts) && lts.Contains(nt))
                                    continue;

                                bool conflict = false;
                                foreach (var gId in dem.BusyGroupIds)
                                {
                                    if (groupSlots.TryGetValue(gId, out var gts) && gts.Contains(nt))
                                    {
                                        conflict = true;
                                        break;
                                    }
                                }
                                if (conflict) continue;

                                bool ntSameWeek = (nt < timeslotsPerWeek) == inWeek0;
                                if (ntSameWeek && !bestSameWeek)
                                {
                                    bestSlot = nt;
                                    bestCount = slotCounts[nt];
                                    bestSameWeek = true;
                                }
                                else if (ntSameWeek == bestSameWeek && slotCounts[nt] < bestCount)
                                {
                                    bestSlot = nt;
                                    bestCount = slotCounts[nt];
                                }
                            }

                            if (bestSlot < 0) continue;

                            // Move the event
                            int oldSlot = t;
                            timeslotAssignments[e] = bestSlot;
                            slotCounts[oldSlot]--;
                            slotCounts[bestSlot]++;

                            lecturerSlots[dem.LecturerId].Remove(oldSlot);
                            lecturerSlots[dem.LecturerId].Add(bestSlot);
                            foreach (var gId in dem.BusyGroupIds)
                            {
                                groupSlots[gId].Remove(oldSlot);
                                groupSlots[gId].Add(bestSlot);
                            }

                            // Update slot events index
                            if (!slotEvents.TryGetValue(bestSlot, out var targetList))
                                slotEvents[bestSlot] = targetList = new List<int>();
                            targetList.Add(e);

                            movedThisPass++;
                        }
                    }

                    totalMoved += movedThisPass;
                    Console.WriteLine($"    Pass {pass + 1}: moved {movedThisPass} (total {totalMoved}), max={slotCounts.Max()}");
                    if (movedThisPass == 0) break;
                }

                int stillOver = slotCounts.Count(c => c > maxPerSlot);

                if (stillOver > 0)
                {
                    Console.WriteLine($"    WARN: {stillOver} slots still over-capacity (max={slotCounts.Max()})");
                    return new ScheduleGeneratorOutput
                    {
                        IsFeasible = false,
                        SolverStatus = $"Phase1.5_StillOverCapacity (max={slotCounts.Max()}, limit={maxPerSlot})"
                    };
                }
            }
            else
            {
                Console.WriteLine($"  [Phase 1.5] No repair needed (max {slotCounts.Max()}/{maxPerSlot} per slot)");
            }

            // =================================================================
            // PHASE 2: Assign classrooms greedily per timeslot
            // =================================================================
            Console.WriteLine($"  [Phase 2] Assigning classrooms...");

            var classroomAssignments = new int[events.Count];
            var failedEvents = new List<int>();

            // Group events by timeslot
            var eventsByTimeslot = new Dictionary<int, List<int>>();
            for (int e = 0; e < events.Count; e++)
            {
                int t = timeslotAssignments[e];
                if (!eventsByTimeslot.TryGetValue(t, out var list))
                {
                    list = new List<int>();
                    eventsByTimeslot[t] = list;
                }
                list.Add(e);
            }

            int preferredCount = 0;
            int totalAssigned = 0;

            foreach (var (timeslot, eventsAtSlot) in eventsByTimeslot.OrderBy(x => x.Key))
            {
                var usedClassrooms = new HashSet<int>();

                // Sort: most constrained first (fewest valid classrooms)
                var sorted = eventsAtSlot
                    .OrderBy(e => demands[events[e].DemandIndex].ValidClassroomIds.Count)
                    .ToList();

                foreach (var e in sorted)
                {
                    var demand = demands[events[e].DemandIndex];
                    var preferred = new HashSet<int>(demand.PreferredClassroomIds);

                    // Try preferred classrooms first, then any valid
                    int? chosen = null;

                    if (preferred.Count > 0)
                    {
                        foreach (var crId in demand.ValidClassroomIds)
                        {
                            if (preferred.Contains(crId) && !usedClassrooms.Contains(crId))
                            {
                                chosen = crId;
                                preferredCount++;
                                break;
                            }
                        }
                    }

                    if (!chosen.HasValue)
                    {
                        foreach (var crId in demand.ValidClassroomIds)
                        {
                            if (!usedClassrooms.Contains(crId))
                            {
                                chosen = crId;
                                break;
                            }
                        }
                    }

                    if (chosen.HasValue)
                    {
                        classroomAssignments[e] = chosen.Value;
                        usedClassrooms.Add(chosen.Value);
                        totalAssigned++;
                    }
                    else
                    {
                        failedEvents.Add(e);
                    }
                }
            }

            Console.WriteLine($"    Assigned: {totalAssigned}/{events.Count}, preferred building: {preferredCount}, failed: {failedEvents.Count}");

            if (failedEvents.Count > 0)
            {
                // Log details about failures
                var failedByTimeslot = failedEvents.GroupBy(e => timeslotAssignments[e]);
                foreach (var g in failedByTimeslot.Take(5))
                {
                    Console.WriteLine($"    Timeslot {g.Key}: {g.Count()} events couldn't get a classroom (slot has {eventsByTimeslot[g.Key].Count} events)");
                }

                return new ScheduleGeneratorOutput
                {
                    IsFeasible = false,
                    SolverStatus = $"Phase2_ClassroomAssignmentFailed ({failedEvents.Count} events)"
                };
            }

            Console.WriteLine($"  [Phase 2] Done ({sw.ElapsedMilliseconds}ms)");

            // =================================================================
            // BUILD OUTPUT
            // =================================================================
            var placed = new List<PlacedLesson>(events.Count);
            for (int e = 0; e < events.Count; e++)
            {
                placed.Add(new PlacedLesson
                {
                    EventIndex = e,
                    DemandIndex = events[e].DemandIndex,
                    Timeslot = timeslotAssignments[e],
                    ClassroomId = classroomAssignments[e]
                });
            }

            return new ScheduleGeneratorOutput
            {
                IsFeasible = true,
                PlacedLessons = placed,
                SolverStatus = status.ToString()
            };
        }
    }

    internal class SolverProgressCallback : CpSolverSolutionCallback
    {
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        public int SolutionCount { get; private set; }

        public override void OnSolutionCallback()
        {
            SolutionCount++;
            Console.WriteLine($"      [Solver] Solution #{SolutionCount}: objective={ObjectiveValue()}, best bound={BestObjectiveBound()}, time={_sw.Elapsed.TotalSeconds:F1}s");
        }
    }
}
