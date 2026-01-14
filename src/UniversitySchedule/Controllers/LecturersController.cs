using Application.Core;
using Application.Features.Lecturers;
using Application.Features.Lecturers.BulkUpload;
using Application.Features.Lecturers.CreateLecturer;
using Application.Features.Lecturers.DeleteLecturer;
using Application.Features.Lecturers.GetAllLecturers;
using Application.Features.Lecturers.GetLecturer;
using Application.Features.Lecturers.UpdateLecturer;
using Application.Models;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniversitySchedule.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LecturersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LecturersController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Create a new lecturer
        /// </summary>
        [HttpPost("Create")]
        public async Task<Result<int>> Create(CreateLecturerCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Get lecturer by ID
        /// </summary>
        [HttpGet("GetById")]
        public async Task<Result<LecturerDto>> GetById([FromQuery] GetLecturerQuery query)
        {
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Get all lecturers with optional filtering and pagination
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<Result<PagedResult<LecturerListDto>>> GetAll([FromQuery] GetAllLecturersQuery query)
        {
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Update an existing lecturer
        /// </summary>
        [HttpPut("Update")]
        public async Task<Result> Update(UpdateLecturerCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Delete a lecturer
        /// </summary>
        [HttpDelete("Delete")]
        public async Task<Result> Delete(DeleteLecturerCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Bulk upload lecturers from CSV file
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

            return await _mediator.Send(new BulkUploadLecturersCommand { CsvContent = content });
        }
    }
}
