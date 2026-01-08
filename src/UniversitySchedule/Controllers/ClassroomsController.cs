using Application.Core;
using Application.Features.Classrooms;
using Application.Features.Classrooms.BulkUpload;
using Application.Features.Classrooms.CreateClassroom;
using Application.Features.Classrooms.DeleteClassroom;
using Application.Features.Classrooms.GetAllClassrooms;
using Application.Features.Classrooms.GetClassroom;
using Application.Features.Classrooms.UpdateClassroom;
using Application.Models;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniversitySchedule.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassroomsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassroomsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Create a new classroom with characteristics
        /// </summary>
        [HttpPost("Create")]
        public async Task<Result<int>> Create(CreateClassroomCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Get classroom by ID with characteristics
        /// </summary>
        [HttpGet("GetById")]
        public async Task<Result<ClassroomDto>> GetById([FromQuery] GetClassroomQuery query)
        {
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Get all classrooms with optional filtering and pagination
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<Result<PagedResult<ClassroomDto>>> GetAll([FromQuery] GetAllClassroomsQuery query)
        {
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Update an existing classroom and its characteristics
        /// </summary>
        [HttpPut("Update")]
        public async Task<Result> Update(UpdateClassroomCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Delete a classroom and its characteristics
        /// </summary>
        [HttpDelete("Delete")]
        public async Task<Result> Delete(DeleteClassroomCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Bulk upload classrooms from CSV file
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

            return await _mediator.Send(new BulkUploadClassroomsCommand { CsvContent = content });
        }
    }
}
