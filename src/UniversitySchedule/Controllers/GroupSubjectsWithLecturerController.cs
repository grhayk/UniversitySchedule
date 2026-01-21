using Application.Core;
using Application.Features.GroupSubjectsWithLecturer;
using Application.Features.GroupSubjectsWithLecturer.AssignLecturerSubjectToGroup;
using Application.Features.GroupSubjectsWithLecturer.BulkUpload;
using Application.Features.GroupSubjectsWithLecturer.GetByGroup;
using Application.Features.GroupSubjectsWithLecturer.GetByLecturerSubject;
using Application.Features.GroupSubjectsWithLecturer.RemoveGroupSubjectWithLecturer;
using Application.Models;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniversitySchedule.Controllers
{
    [ApiController]
    [Route("api")]
    public class GroupSubjectsWithLecturerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GroupSubjectsWithLecturerController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Assign a lecturer-subject to a group with a specific lesson type
        /// </summary>
        [HttpPost("groups/{groupId}/lecturer-subjects")]
        public async Task<Result<int>> AssignLecturerSubjectToGroup(int groupId, [FromBody] AssignLecturerSubjectToGroupRequest request)
        {
            var command = new AssignLecturerSubjectToGroupCommand
            {
                GroupId = groupId,
                LecturerSubjectId = request.LecturerSubjectId,
                LessonType = request.LessonType
            };
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Get all lecturer-subjects assigned to a group
        /// </summary>
        [HttpGet("groups/{groupId}/lecturer-subjects")]
        public async Task<Result<List<GroupLecturerSubjectListDto>>> GetByGroup(int groupId, [FromQuery] LessonType? lessonType)
        {
            return await _mediator.Send(new GetByGroupQuery
            {
                GroupId = groupId,
                LessonType = lessonType
            });
        }

        /// <summary>
        /// Get all groups assigned to a lecturer-subject
        /// </summary>
        [HttpGet("lecturer-subjects/{lecturerSubjectId}/groups")]
        public async Task<Result<List<LecturerSubjectGroupListDto>>> GetByLecturerSubject(int lecturerSubjectId, [FromQuery] LessonType? lessonType)
        {
            return await _mediator.Send(new GetByLecturerSubjectQuery
            {
                LecturerSubjectId = lecturerSubjectId,
                LessonType = lessonType
            });
        }

        /// <summary>
        /// Remove a group-subject-lecturer assignment
        /// </summary>
        [HttpDelete("group-subjects-with-lecturer/{id}")]
        public async Task<Result> Remove(int id)
        {
            return await _mediator.Send(new RemoveGroupSubjectWithLecturerCommand { Id = id });
        }

        /// <summary>
        /// Bulk upload group-subject-lecturer assignments from CSV file
        /// </summary>
        [HttpPost("group-subjects-with-lecturer/bulk-upload")]
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

            return await _mediator.Send(new BulkUploadGroupSubjectsWithLecturerCommand { CsvContent = content });
        }
    }

    public record AssignLecturerSubjectToGroupRequest
    {
        public int LecturerSubjectId { get; init; }
        public LessonType LessonType { get; init; }
    }
}
