using Application.Core;
using Application.Features.Students;
using Application.Features.Students.BulkUpload;
using Application.Features.Students.CreateStudent;
using Application.Features.Students.DeleteStudent;
using Application.Features.Students.GetAllStudents;
using Application.Features.Students.GetStudent;
using Application.Features.Students.UpdateStudent;
using Application.Models;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniversitySchedule.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Create a new student
        /// </summary>
        [HttpPost("Create")]
        public async Task<Result<int>> Create(CreateStudentCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Get student by ID
        /// </summary>
        [HttpGet("GetById")]
        public async Task<Result<StudentDto>> GetById([FromQuery] GetStudentQuery query)
        {
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Get all students with optional filtering and pagination
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<Result<PagedResult<StudentListDto>>> GetAll([FromQuery] GetAllStudentsQuery query)
        {
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Update an existing student
        /// </summary>
        [HttpPut("Update")]
        public async Task<Result> Update(UpdateStudentCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Delete a student
        /// </summary>
        [HttpDelete("Delete")]
        public async Task<Result> Delete(DeleteStudentCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Bulk upload students from CSV file
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

            return await _mediator.Send(new BulkUploadStudentsCommand { CsvContent = content });
        }
    }
}
