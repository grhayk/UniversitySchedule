using Application.Interfaces;
using Application.Models.ScheduleGeneration;
using Google.OrTools.Sat;

namespace Infrastructure.Services
{
    public class ScheduleGenerator : IScheduleGenerator
    {
        public ScheduleGeneratorOutput Generate(ScheduleGeneratorInput input)
        {
            var model = new CpModel();
            var demands = input.Demands;
            var events = input.Events;
            int totalTimeslots = input.TotalTimeslots;
            int slotsPerDay = input.SlotsPerDay;
            int timeslotsPerWeek = input.TimeslotsPerWeek;

            if (events.Count == 0)
            {
                return new ScheduleGeneratorOutput
                {
                    IsFeasible = true,
                    SolverStatus = "NoEvents"
                };
            }

            // =============================================
            // VARIABLES
            // =============================================

            // For each event: timeslot [0, totalTimeslots-1] and classroom (from valid set)
            var timeslotVars = new IntVar[events.Count];
            var classroomVars = new IntVar[events.Count];

            // We also keep an index var per event mapping to the demand's valid classroom list
            var classroomIndexVars = new IntVar[events.Count];

            for (int e = 0; e < events.Count; e++)
            {
                var demand = demands[events[e].DemandIndex];

                timeslotVars[e] = model.NewIntVar(0, totalTimeslots - 1, $"ts_{e}");

                // Classroom: use index into valid classroom list, then map to actual ID
                var validCr = demand.ValidClassroomIds;
                if (validCr.Count == 1)
                {
                    // Only one valid classroom — fix it
                    classroomVars[e] = model.NewIntVar(validCr[0], validCr[0], $"cr_{e}");
                    classroomIndexVars[e] = model.NewIntVar(0, 0, $"crIdx_{e}");
                }
                else
                {
                    classroomIndexVars[e] = model.NewIntVar(0, validCr.Count - 1, $"crIdx_{e}");
                    classroomVars[e] = model.NewIntVar(validCr.Min(), validCr.Max(), $"cr_{e}");

                    // Map index to actual classroom ID using allowed assignments
                    var table = model.AddAllowedAssignments(new[] { classroomIndexVars[e], classroomVars[e] });
                    for (int j = 0; j < validCr.Count; j++)
                    {
                        table.AddTuple(new long[] { j, validCr[j] });
                    }
                }
            }

            // =============================================
            // HARD CONSTRAINT 1: Classroom no-overlap
            // =============================================
            // combined[e] = timeslot[e] * maxCrId + classroom[e]
            // AddAllDifferent(combined) ensures no two events share (timeslot, classroom)
            int maxCrId = input.AllClassroomIds.Max() + 1;
            var combined = new IntVar[events.Count];
            for (int e = 0; e < events.Count; e++)
            {
                long maxCombined = (long)(totalTimeslots - 1) * maxCrId + maxCrId - 1;
                combined[e] = model.NewIntVar(0, maxCombined, $"comb_{e}");
                model.Add(combined[e] == timeslotVars[e] * maxCrId + classroomVars[e]);
            }
            model.AddAllDifferent(combined);

            // =============================================
            // HARD CONSTRAINT 2: Lecturer no-overlap
            // =============================================
            // Events of the same lecturer must have different timeslots
            var eventsByLecturer = events
                .Select((ev, i) => (ev, i))
                .GroupBy(x => demands[x.ev.DemandIndex].LecturerId);

            foreach (var group in eventsByLecturer)
            {
                var indices = group.Select(x => x.i).ToArray();
                if (indices.Length > 1)
                    model.AddAllDifferent(indices.Select(i => timeslotVars[i]).ToArray());
            }

            // =============================================
            // HARD CONSTRAINT 3: Group no-overlap
            // =============================================
            // For each busy group, events whose BusyGroupIds contains it must have different timeslots
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

            foreach (var (_, eventIndices) in eventsByBusyGroup)
            {
                if (eventIndices.Count > 1)
                    model.AddAllDifferent(eventIndices.Select(i => timeslotVars[i]).ToArray());
            }

            // =============================================
            // HARD CONSTRAINT 4: Hours distribution per demand
            // =============================================
            // isInWeek0[e] = true if timeslot < timeslotsPerWeek (numerator week)
            var isInWeek0 = new BoolVar[events.Count];
            for (int e = 0; e < events.Count; e++)
            {
                isInWeek0[e] = model.NewBoolVar($"w0_{e}");
                model.Add(timeslotVars[e] < timeslotsPerWeek).OnlyEnforceIf(isInWeek0[e]);
                model.Add(timeslotVars[e] >= timeslotsPerWeek).OnlyEnforceIf(isInWeek0[e].Not());
            }

            // Group events by demand and constrain week distribution
            var eventsByDemand = events
                .Select((ev, i) => (ev, i))
                .GroupBy(x => x.ev.DemandIndex);

            foreach (var grp in eventsByDemand)
            {
                var demand = demands[grp.Key];
                var evIndices = grp.Select(x => x.i).ToArray();
                int H = demand.Hours;
                int hi = (H + 1) / 2; // ceil(H/2)
                int lo = H / 2;       // floor(H/2)

                var week0Count = LinearExpr.Sum(evIndices.Select(i => isInWeek0[i]));

                if (hi == lo)
                {
                    // Even hours: exactly H/2 in each week
                    model.Add(week0Count == hi);
                }
                else
                {
                    // Odd hours: week0 gets hi or lo (solver decides)
                    var pickHi = model.NewBoolVar($"pickHi_{grp.Key}");
                    model.Add(week0Count == hi).OnlyEnforceIf(pickHi);
                    model.Add(week0Count == lo).OnlyEnforceIf(pickHi.Not());
                }

                // Prevent two events of same demand on same day-in-week
                // dayInSchedule[e] = timeslot / slotsPerDay (0-9 across 2 weeks)
                if (evIndices.Length > 1)
                {
                    var dayVars = new IntVar[evIndices.Length];
                    for (int i = 0; i < evIndices.Length; i++)
                    {
                        dayVars[i] = model.NewIntVar(0, (totalTimeslots / slotsPerDay) - 1, $"day_{evIndices[i]}");
                        model.AddDivisionEquality(dayVars[i], timeslotVars[evIndices[i]], slotsPerDay);
                    }
                    model.AddAllDifferent(dayVars);
                }
            }

            // =============================================
            // SOFT CONSTRAINTS (Objective)
            // =============================================

            var penalties = new List<IntVar>();

            // Penalty for late slots (slot 4 = small penalty, slot 5 = larger)
            for (int e = 0; e < events.Count; e++)
            {
                var slotInDay = model.NewIntVar(0, slotsPerDay - 1, $"slot_{e}");
                model.AddModuloEquality(slotInDay, timeslotVars[e], slotsPerDay);

                var isSlot4 = model.NewBoolVar($"s4_{e}");
                model.Add(slotInDay == slotsPerDay - 2).OnlyEnforceIf(isSlot4);
                model.Add(slotInDay != slotsPerDay - 2).OnlyEnforceIf(isSlot4.Not());

                var isSlot5 = model.NewBoolVar($"s5_{e}");
                model.Add(slotInDay == slotsPerDay - 1).OnlyEnforceIf(isSlot5);
                model.Add(slotInDay != slotsPerDay - 1).OnlyEnforceIf(isSlot5.Not());

                var penalty = model.NewIntVar(0, 3, $"pen_{e}");
                model.Add(penalty == isSlot4 + 3 * isSlot5);
                penalties.Add(penalty);
            }

            model.Minimize(LinearExpr.Sum(penalties));

            // =============================================
            // SOLVE
            // =============================================
            var solver = new CpSolver();
            solver.StringParameters = $"max_time_in_seconds:{input.TimeLimitSeconds}";

            var status = solver.Solve(model);

            if (status != CpSolverStatus.Optimal && status != CpSolverStatus.Feasible)
            {
                return new ScheduleGeneratorOutput
                {
                    IsFeasible = false,
                    SolverStatus = status.ToString()
                };
            }

            // =============================================
            // EXTRACT SOLUTION
            // =============================================
            var placed = new List<PlacedLesson>(events.Count);
            for (int e = 0; e < events.Count; e++)
            {
                placed.Add(new PlacedLesson
                {
                    EventIndex = e,
                    DemandIndex = events[e].DemandIndex,
                    Timeslot = (int)solver.Value(timeslotVars[e]),
                    ClassroomId = (int)solver.Value(classroomVars[e])
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
}
