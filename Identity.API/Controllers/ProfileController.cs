using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Profile.Commands.AdminUpdateUser;
using Identity.Application.Features.Profile.Commands.UpdateProfile;
using Identity.Application.Features.Profile.Dtos.AdminUpdateUser;
using Identity.Application.Features.Profile.Dtos.UpdateProfile;
using Identity.Application.Features.Profile.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly IUser _currentUser;

        public ProfileController(IMediator mediator, IUser currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        /// <summary>Get the authenticated user's own profile</summary>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
        {
            if (_currentUser.Id is null)
                return Unauthorized();

            var result = await _mediator.Send(new GetProfileQuery(_currentUser.Id.Value), cancellationToken);

            return result.Match<IActionResult>(
                onValue: profile => OkEnvelope(profile, "Profile retrieved successfully."),
                onError: errors => Problem(errors)
            );
        }

        /// <summary>Update the authenticated user's own profile (Name, Phone Number)</summary>
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] UpdateProfileRequest request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.Id is null)
                return Unauthorized();

            var command = new UpdateProfileCommand(_currentUser.Id.Value, request);
            var result = await _mediator.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                onValue: profile => OkEnvelope(profile, "Profile updated successfully."),
                onError: errors => Problem(errors)
            );
        }

        /// <summary>Update another user's profile information (Admin only)</summary>
        [HttpPut("{userId:guid}/admin")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AdminUpdateUser(
            Guid userId,
            [FromBody] AdminUpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            var command = new AdminUpdateUserCommand(userId, request);
            var result = await _mediator.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                onValue: profile => OkEnvelope(profile, "User profile updated successfully by admin."),
                onError: errors => Problem(errors)
            );
        }
    }
}