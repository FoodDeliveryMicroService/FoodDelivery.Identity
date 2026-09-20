using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Authentication.Commands.SendConfirmationCode;
using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Application.Features.Authentication.Dtos.RegisterUser;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IIdentityService identityService,
    IMediator _mediator,
    ILogger<RegisterUserCommandHandler> logger)
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Log that we are starting the registration process
        logger.LogInformation("Registering user with email: {Email}", request.Email);

        // Delegate the actual creation to the Identity Service
        var creationResult = await identityService.CreateUserAsync(
            request.Name,
            request.Email,
            request.PhoneNumber,
            request.Password,
            request.Role.ToString(),
            cancellationToken);

        // If the creation fails, return the errors directly
        if (creationResult.IsError)
        {
            // Return the list of errors – implicit conversion to Result<RegisterUserResponse>
            return creationResult.Errors;
        }

        // Success: return the response without tokens (as required by FR-01)
        var userDto = creationResult.Value;
        logger.LogInformation("User registered successfully with ID: {UserId}", userDto.UserId);

        // 2. Send email confirmation code
        var confirmationResult = await _mediator.Send(
            new SendConfirmationCodeCommand(
                new SendConfirmationCodeRequest(request.Email)),
            cancellationToken);

        if (confirmationResult.IsError)
        {
            logger.LogError(
                "User {UserId} was registered successfully, but confirmation email could not be sent.",
                userDto.UserId);

            return confirmationResult.Errors;
        }

        // 3. Return registration response
        return new RegisterUserResponse(
            userDto.UserId,
            userDto.Email!,
            request.Name);
    }
}