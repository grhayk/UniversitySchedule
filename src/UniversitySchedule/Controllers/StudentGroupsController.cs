using Application.Core;
using Application.Features.StudentGroups;
using Application.Features.StudentGroups.AssignStudentToGroup;
using Application.Features.StudentGroups.BulkUpload;
using Application.Features.StudentGroups.GetGroupsByStudent;
using Application.Features.StudentGroups.GetStudentsByGroup;
using Application.Features.StudentGroups.RemoveStudentFromGroup;
using Application.Models;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniversitySchedule.Controllers
{
    [ApiController]
    [Route("api")]
    public class StudentGroupsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentGroupsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Assign a student to a group (can be subgroup - practical/lab)
        /// </summary>
        [HttpPost("students/{studentId}/groups")]
        public async Task<Result<int>> AssignToGroup(int studentId, [FromBody] AssignToGroupRequest request)
        {
            var command = new AssignStudentToGroupCommand
            {
                StudentId = studentId,
                GroupId = request.GroupId
            };
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Get all groups a student belongs to
        /// </summary>
        [HttpGet("students/{studentId}/groups")]
        public async Task<Result<List<StudentGroupDto>>> GetGroupsByStudent(int studentId)
        {
            return await _mediator.Send(new GetGroupsByStudentQuery { StudentId = studentId });
        }

        /// <summary>
        /// Get all students in a group
        /// </summary>
        [HttpGet("groups/{groupId}/students")]
        public async Task<Result<PagedResult<StudentGroupListDto>>> GetStudentsByGroup(
            int groupId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetStudentsByGroupQuery
            {
                GroupId = groupId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Remove a student from a group (cannot remove from main/parent group)
        /// </summary>
        [HttpDelete("student-groups/{id}")]
        public async Task<Result> Remove(int id)
        {
            return await _mediator.Send(new RemoveStudentFromGroupCommand { Id = id });
        }

        /// <summary>
        /// Bulk upload student-group assignments from CSV file
        /// </summary>
        [HttpPost("student-groups/bulk-upload")]
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

            return await _mediator.Send(new BulkUploadStudentGroupsCommand { CsvContent = content });
        }
    }

    public record AssignToGroupRequest
    {
        public int GroupId { get; init; }
    }
}
