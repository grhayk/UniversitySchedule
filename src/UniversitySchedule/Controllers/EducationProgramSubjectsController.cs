using Application.Core;
using Application.Features.EducationProgramSubjects;
using Application.Features.EducationProgramSubjects.AssignSubjectToProgram;
using Application.Features.EducationProgramSubjects.BulkUpload;
using Application.Features.EducationProgramSubjects.GetProgramsBySubject;
using Application.Features.EducationProgramSubjects.GetSubjectsByProgram;
using Application.Features.EducationProgramSubjects.RemoveProgramSubject;
using Application.Features.EducationProgramSubjects.UpdateProgramSubject;
using Application.Models;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniversitySchedule.Controllers
{
    [ApiController]
    [Route("api")]
    public class EducationProgramSubjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EducationProgramSubjectsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Assign a subject to an education program
        /// </summary>
        [HttpPost("education-programs/{programId}/subjects")]
        public async Task<Result<int>> AssignSubject(int programId, [FromBody] AssignSubjectRequest request)
        {
            var command = new AssignSubjectToProgramCommand
            {
                EducationProgramId = programId,
                SubjectId = request.SubjectId,
                SemesterId = request.SemesterId,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            };
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Get all subjects assigned to an education program
        /// </summary>
        [HttpGet("education-programs/{programId}/subjects")]
        public async Task<Result<PagedResult<ProgramSubjectDto>>> GetSubjectsByProgram(
            int programId,
            [FromQuery] int? semesterId,
            [FromQuery] bool activeOnly = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetSubjectsByProgramQuery
            {
                EducationProgramId = programId,
                SemesterId = semesterId,
                ActiveOnly = activeOnly,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Get all education programs that include a subject (useful for impact analysis)
        /// </summary>
        [HttpGet("subjects/{subjectId}/programs")]
        public async Task<Result<PagedResult<SubjectProgramDto>>> GetProgramsBySubject(
            int subjectId,
            [FromQuery] bool activeOnly = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetProgramsBySubjectQuery
            {
                SubjectId = subjectId,
                ActiveOnly = activeOnly,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Update a program-subject assignment (semester, dates)
        /// </summary>
        [HttpPut("education-program-subjects/{id}")]
        public async Task<Result> Update(int id, [FromBody] UpdateProgramSubjectRequest request)
        {
            var command = new UpdateProgramSubjectCommand
            {
                Id = id,
                SemesterId = request.SemesterId,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            };
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Remove a subject from an education program
        /// </summary>
        [HttpDelete("education-program-subjects/{id}")]
        public async Task<Result> Remove(int id)
        {
            return await _mediator.Send(new RemoveProgramSubjectCommand { Id = id });
        }

        /// <summary>
        /// Bulk upload program-subject assignments from CSV file
        /// </summary>
        [HttpPost("education-program-subjects/bulk-upload")]
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

            return await _mediator.Send(new BulkUploadProgramSubjectsCommand { CsvContent = content });
        }
    }

    // Request DTOs for cleaner API
    public record AssignSubjectRequest
    {
        public int SubjectId { get; init; }
        public int SemesterId { get; init; }
        public DateTime FromDate { get; init; }
        public DateTime? ToDate { get; init; }
    }

    public record UpdateProgramSubjectRequest
    {
        public int SemesterId { get; init; }
        public DateTime FromDate { get; init; }
        public DateTime? ToDate { get; init; }
    }
}
