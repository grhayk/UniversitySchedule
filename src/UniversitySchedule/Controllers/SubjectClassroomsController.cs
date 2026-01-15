using Application.Core;
using Application.Features.SubjectClassrooms;
using Application.Features.SubjectClassrooms.AssignClassroomToSubject;
using Application.Features.SubjectClassrooms.BulkUpload;
using Application.Features.SubjectClassrooms.GetClassroomsBySubject;
using Application.Features.SubjectClassrooms.GetSubjectsByClassroom;
using Application.Features.SubjectClassrooms.RemoveSubjectClassroom;
using Application.Models;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniversitySchedule.Controllers
{
    [ApiController]
    [Route("api")]
    public class SubjectClassroomsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubjectClassroomsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Assign a classroom to a subject for a specific lesson type
        /// </summary>
        [HttpPost("subjects/{subjectId}/classrooms")]
        public async Task<Result<int>> AssignClassroom(int subjectId, [FromBody] AssignClassroomRequest request)
        {
            var command = new AssignClassroomToSubjectCommand
            {
                SubjectId = subjectId,
                LessonType = request.LessonType,
                ClassroomId = request.ClassroomId
            };
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Get all classrooms assigned to a subject
        /// </summary>
        [HttpGet("subjects/{subjectId}/classrooms")]
        public async Task<Result<List<SubjectClassroomListDto>>> GetClassroomsBySubject(
            int subjectId,
            [FromQuery] LessonType? lessonType)
        {
            var query = new GetClassroomsBySubjectQuery
            {
                SubjectId = subjectId,
                LessonType = lessonType
            };
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Get all subjects that use a classroom
        /// </summary>
        [HttpGet("classrooms/{classroomId}/subjects")]
        public async Task<Result<List<ClassroomSubjectListDto>>> GetSubjectsByClassroom(
            int classroomId,
            [FromQuery] LessonType? lessonType)
        {
            var query = new GetSubjectsByClassroomQuery
            {
                ClassroomId = classroomId,
                LessonType = lessonType
            };
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Remove a classroom from a subject
        /// </summary>
        [HttpDelete("subject-classrooms/{id}")]
        public async Task<Result> Remove(int id)
        {
            return await _mediator.Send(new RemoveSubjectClassroomCommand { Id = id });
        }

        /// <summary>
        /// Bulk upload subject-classroom assignments from CSV file
        /// </summary>
        [HttpPost("subject-classrooms/bulk-upload")]
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

            return await _mediator.Send(new BulkUploadSubjectClassroomsCommand { CsvContent = content });
        }
    }

    public record AssignClassroomRequest
    {
        public LessonType LessonType { get; init; }
        public int ClassroomId { get; init; }
    }
}
