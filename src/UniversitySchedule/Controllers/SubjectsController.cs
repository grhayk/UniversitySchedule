using Application.Core;
using Application.Features.Subjects;
using Application.Features.Subjects.BulkUpload;
using Application.Features.Subjects.CreateSubject;
using Application.Features.Subjects.DeleteSubject;
using Application.Features.Subjects.GetAllSubjects;
using Application.Features.Subjects.GetSubject;
using Application.Features.Subjects.UpdateSubject;
using Application.Models;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniversitySchedule.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubjectsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Create a new subject with configs
        /// </summary>
        [HttpPost("Create")]
        public async Task<Result<int>> Create(CreateSubjectCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Get subject by ID with configs
        /// </summary>
        [HttpGet("GetById")]
        public async Task<Result<SubjectDto>> GetById([FromQuery] GetSubjectQuery query)
        {
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Get all subjects with optional filtering and pagination
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<Result<PagedResult<SubjectDto>>> GetAll([FromQuery] GetAllSubjectsQuery query)
        {
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Update an existing subject and its configs
        /// </summary>
        [HttpPut("Update")]
        public async Task<Result> Update(UpdateSubjectCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Delete a subject and its configs
        /// </summary>
        [HttpDelete("Delete")]
        public async Task<Result> Delete(DeleteSubjectCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Bulk upload subjects from CSV file
        /// </summary>
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

            return await _mediator.Send(new BulkUploadSubjectsCommand { CsvContent = content });
        }
    }
}
