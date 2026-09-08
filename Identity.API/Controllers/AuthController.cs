using Identity.Application.Features.Authentication.Commands.ConfirmEmail;
using Identity.Application.Features.Authentication.Commands.RegisterUser;
using Identity.Application.Features.Authentication.Commands.SendConfirmationCode;
using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Application.Features.Authentication.Dtos.RegisterUser;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ApiController
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Register a new user (Customer or RestaurantOwner)
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Newly created user details</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(
            [FromForm] RegisterUserRequest request,
            CancellationToken cancellationToken)
        {
            // Create the command from the request
            var command = new RegisterUserCommand(request);

            // Send the command to MediatR
            var result = await _mediator.Send(command, cancellationToken);

            // Use the Match method to handle success and failure
            return result.Match<IActionResult>(
                onValue: response => CreatedEnvelope(
                    response,
                    "User registered successfully. Please check your email to confirm your account."
                ),
                onError: errors => Problem(errors)
            );
        }

        [HttpPost("confirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ConfirmEmail(
            [FromBody] ConfirmEmailRequest request,
            CancellationToken cancellationToken)
        {
            var command = new ConfirmEmailCommand(request);
            var result = await _mediator.Send(command, cancellationToken);

            return result.Match<IActionResult>(
                onValue: success => OkEnvelope(
                    new { message = "Email confirmed successfully." },
                    "Your email has been verified. You can now log in."
                ),
                onError: errors => Problem(errors)
            );
        }
    }
}