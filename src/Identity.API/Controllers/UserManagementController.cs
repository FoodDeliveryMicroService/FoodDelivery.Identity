using Identity.Application.Features.AccountStatus.Commands.ReactivateUser;
using Identity.Application.Features.AccountStatus.Commands.SuspendUser;
using Identity.Application.Features.RoleManagement.Commands.ChangeUserRole;
using Identity.Application.Features.RoleManagement.Dtos.ChangeUserRole;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class UserManagementController : ApiController
    {
        private readonly IMediator _mediator;

        public UserManagementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Assign or modify a user's role (Admin only)
        /// </summary>
        [HttpPut("{userId:guid}/role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ChangeUserRole(
            Guid userId,
            [FromForm] ChangeUserRoleRequest request,
            CancellationToken cancellationToken)
        {
            var command = new ChangeUserRoleCommand(userId, request);

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                onValue: response => OkEnvelope(response, "User role updated successfully."),
                onError: errors => Problem(errors)
            );
        }

        [HttpPut("{userId:guid}/suspend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SuspendUser(
    Guid userId,
    CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new SuspendUserCommand(userId), cancellationToken);

            return result.Match<IActionResult>(
                onValue: response => OkEnvelope(response, "User account suspended successfully."),
                onError: errors => Problem(errors)
            );
        }

        [HttpPut("{userId:guid}/reactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ReactivateUser(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ReactivateUserCommand(userId), cancellationToken);

            return result.Match<IActionResult>(
                onValue: response => OkEnvelope(response, "User account reactivated successfully."),
                onError: errors => Problem(errors)
            );
        }
    }
}