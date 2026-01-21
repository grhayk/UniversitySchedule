using Application.Core;
using Application.Features.Schedules;
using Application.Features.Schedules.BulkUpload;
using Application.Features.Schedules.CreateSchedule;
using Application.Features.Schedules.DeleteSchedule;
using Application.Features.Schedules.GetScheduleById;
using Application.Features.Schedules.GetSchedulesByGroup;
using Application.Features.Schedules.GetSchedulesBySemester;
using Application.Features.Schedules.UpdateSchedule;
using Application.Models;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniversitySchedule.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SchedulesController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Create a new schedule entry
        /// </summary>
        [HttpPost]
        public async Task<Result<int>> Create([FromBody] CreateScheduleRequest request)
        {
            var command = new CreateScheduleCommand
            {
                SubjectId = request.SubjectId,
                LecturerId = request.LecturerId,
                LessonType = request.LessonType,
                ClassroomId = request.ClassroomId,
                TimeTableId = request.TimeTableId,
                WeekType = request.WeekType,
                ScheduleDate = request.ScheduleDate,
                SemesterId = request.SemesterId,
                ScheduleParentId = request.ScheduleParentId,
                GroupIds = request.GroupIds
            };
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Get schedule by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<Result<ScheduleDetailDto>> GetById(int id)
        {
            return await _mediator.Send(new GetScheduleByIdQuery { Id = id });
        }

        /// <summary>
        /// Get schedules by group with optional filters
        /// </summary>
        [HttpGet("by-group/{groupId}")]
        public async Task<Result<List<ScheduleListDto>>> GetByGroup(
            int groupId,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] LessonType? lessonType,
            [FromQuery] WeekType? weekType)
        {
            return await _mediator.Send(new GetSchedulesByGroupQuery
            {
                GroupId = groupId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                LessonType = lessonType,
                WeekType = weekType
            });
        }

        /// <summary>
        /// Get schedules by semester with optional filters
        /// </summary>
        [HttpGet("by-semester/{semesterId}")]
        public async Task<Result<List<ScheduleListDto>>> GetBySemester(
            int semesterId,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] LessonType? lessonType,
            [FromQuery] WeekType? weekType,
            [FromQuery] int? lecturerId,
            [FromQuery] int? subjectId)
        {
            return await _mediator.Send(new GetSchedulesBySemesterQuery
            {
                SemesterId = semesterId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                LessonType = lessonType,
                WeekType = weekType,
                LecturerId = lecturerId,
                SubjectId = subjectId
            });
        }

        /// <summary>
        /// Update a schedule entry
        /// </summary>
        [HttpPut("{id}")]
        public async Task<Result> Update(int id, [FromBody] UpdateScheduleRequest request)
        {
            var command = new UpdateScheduleCommand
            {
                Id = id,
                SubjectId = request.SubjectId,
                LecturerId = request.LecturerId,
                LessonType = request.LessonType,
                ClassroomId = request.ClassroomId,
                TimeTableId = request.TimeTableId,
                WeekType = request.WeekType,
                ScheduleDate = request.ScheduleDate,
                SemesterId = request.SemesterId,
                ScheduleParentId = request.ScheduleParentId,
                GroupIds = request.GroupIds
            };
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Delete a schedule entry
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<Result> Delete(int id)
        {
            return await _mediator.Send(new DeleteScheduleCommand { Id = id });
        }

        /// <summary>
        /// Bulk upload schedules from CSV file
        /// </summary>
        /// <remarks>
        /// CSV format: SubjectId,LecturerId,LessonType,ClassroomId,TimeTableId,WeekType,ScheduleDate,SemesterId,ScheduleParentId,GroupIds
        /// GroupIds should be pipe-separated (e.g., "1|2|3")
        /// </remarks>
        [HttpPost("bulk-upload")]
        [Consumes("multipart/form-data")]
        public async Task<Result<BulkUploadResult>> BulkUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Result.Failure<BulkUploadResult>(ErrorType.Validation, "No file provided");
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<BulkUploadResult>(ErrorType.Validation, "Only CSV files are supported");
            }

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            return await _mediator.Send(new BulkUploadScheduleCommand { CsvContent = content });
        }
    }

    public record CreateScheduleRequest
    {
        public int SubjectId { get; init; }
        public int LecturerId { get; init; }
        public LessonType LessonType { get; init; }
        public int ClassroomId { get; init; }
        public int TimeTableId { get; init; }
        public WeekType WeekType { get; init; }
        public DateTime ScheduleDate { get; init; }
        public int SemesterId { get; init; }
        public int? ScheduleParentId { get; init; }
        public List<int> GroupIds { get; init; } = new();
    }

    public record UpdateScheduleRequest
    {
        public int SubjectId { get; init; }
        public int LecturerId { get; init; }
        public LessonType LessonType { get; init; }
        public int ClassroomId { get; init; }
        public int TimeTableId { get; init; }
        public WeekType WeekType { get; init; }
        public DateTime ScheduleDate { get; init; }
        public int SemesterId { get; init; }
        public int? ScheduleParentId { get; init; }
        public List<int> GroupIds { get; init; } = new();
    }
}
