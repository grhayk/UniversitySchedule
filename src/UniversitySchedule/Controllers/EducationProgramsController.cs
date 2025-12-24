using Application.Core;
using Application.Features.EducationPrograms;
using Application.Features.EducationPrograms.BulkUpload;
using Application.Features.EducationPrograms.CreateEducationProgram;
using Application.Features.EducationPrograms.DeleteEducationProgram;
using Application.Features.EducationPrograms.GetAllEducationPrograms;
using Application.Features.EducationPrograms.GetEducationProgram;
using Application.Features.EducationPrograms.UpdateEducationProgram;
using Application.Models;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniversitySchedule.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EducationProgramsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EducationProgramsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Create a new education program
        /// </summary>
        [HttpPost("Create")]
        public async Task<Result<int>> Create(CreateEducationProgramCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Get education program by ID
        /// </summary>
        [HttpGet("GetById")]
        public async Task<Result<EducationProgramDto>> GetById([FromQuery] GetEducationProgramQuery query)
        {
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Get all education programs with optional filtering
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<Result<List<EducationProgramDto>>> GetAll([FromQuery] GetAllEducationProgramsQuery query)
        {
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Update an existing education program
        /// </summary>
        [HttpPut("Update")]
        public async Task<Result> Update(UpdateEducationProgramCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Delete an education program
        /// </summary>
        [HttpDelete("Delete")]
        public async Task<Result> Delete(DeleteEducationProgramCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Bulk upload education programs from CSV file
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

            return await _mediator.Send(new BulkUploadEducationProgramsCommand { CsvContent = content });
        }
    }
}
