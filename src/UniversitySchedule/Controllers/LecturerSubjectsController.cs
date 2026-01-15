using Application.Core;
using Application.Features.LecturerSubjects;
using Application.Features.LecturerSubjects.AssignSubjectToLecturer;
using Application.Features.LecturerSubjects.BulkUpload;
using Application.Features.LecturerSubjects.GetLecturersBySubject;
using Application.Features.LecturerSubjects.GetSubjectsByLecturer;
using Application.Features.LecturerSubjects.RemoveLecturerSubject;
using Application.Models;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniversitySchedule.Controllers
{
    [ApiController]
    [Route("api")]
    public class LecturerSubjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LecturerSubjectsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Assign a subject to a lecturer
        /// </summary>
        [HttpPost("lecturers/{lecturerId}/subjects")]
        public async Task<Result<int>> AssignSubject(int lecturerId, [FromBody] AssignSubjectToLecturerRequest request)
        {
            var command = new AssignSubjectToLecturerCommand
            {
                LecturerId = lecturerId,
                SubjectId = request.SubjectId
            };
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Get all subjects assigned to a lecturer
        /// </summary>
        [HttpGet("lecturers/{lecturerId}/subjects")]
        public async Task<Result<List<LecturerSubjectListDto>>> GetSubjectsByLecturer(int lecturerId)
        {
            return await _mediator.Send(new GetSubjectsByLecturerQuery { LecturerId = lecturerId });
        }

        /// <summary>
        /// Get all lecturers that can teach a subject
        /// </summary>
        [HttpGet("subjects/{subjectId}/lecturers")]
        public async Task<Result<List<SubjectLecturerListDto>>> GetLecturersBySubject(int subjectId)
        {
            return await _mediator.Send(new GetLecturersBySubjectQuery { SubjectId = subjectId });
        }

        /// <summary>
        /// Remove a subject from a lecturer
        /// </summary>
        [HttpDelete("lecturer-subjects/{id}")]
        public async Task<Result> Remove(int id)
        {
            return await _mediator.Send(new RemoveLecturerSubjectCommand { Id = id });
        }

        /// <summary>
        /// Bulk upload lecturer-subject assignments from CSV file
        /// </summary>
        [HttpPost("lecturer-subjects/bulk-upload")]
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

            return await _mediator.Send(new BulkUploadLecturerSubjectsCommand { CsvContent = content });
        }
    }

    public record AssignSubjectToLecturerRequest
    {
        public int SubjectId { get; init; }
    }
}
