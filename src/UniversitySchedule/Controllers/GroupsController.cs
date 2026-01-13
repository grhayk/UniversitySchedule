using Application.Core;
using Application.Features.Groups;
using Application.Features.Groups.BulkUpload;
using Application.Features.Groups.CreateGroup;
using Application.Features.Groups.DeleteGroup;
using Application.Features.Groups.GetAllGroups;
using Application.Features.Groups.GetGroup;
using Application.Features.Groups.UpdateGroup;
using Application.Models;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UniversitySchedule.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GroupsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Create a new group
        /// </summary>
        [HttpPost("Create")]
        public async Task<Result<int>> Create(CreateGroupCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Get group by ID with children
        /// </summary>
        [HttpGet("GetById")]
        public async Task<Result<GroupDto>> GetById([FromQuery] GetGroupQuery query)
        {
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Get all groups with optional filtering and pagination
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<Result<PagedResult<GroupListDto>>> GetAll([FromQuery] GetAllGroupsQuery query)
        {
            return await _mediator.Send(query);
        }

        /// <summary>
        /// Update an existing group
        /// </summary>
        [HttpPut("Update")]
        public async Task<Result> Update(UpdateGroupCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Delete a group (must not have children)
        /// </summary>
        [HttpDelete("Delete")]
        public async Task<Result> Delete(DeleteGroupCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <summary>
        /// Bulk upload groups from CSV file
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

            return await _mediator.Send(new BulkUploadGroupsCommand { CsvContent = content });
        }
    }
}
