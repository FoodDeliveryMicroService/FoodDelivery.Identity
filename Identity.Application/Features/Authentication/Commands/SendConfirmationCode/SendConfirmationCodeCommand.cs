using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.SendConfirmationCode
{
    public sealed record SendConfirmationCodeCommand(SendConfirmationCodeRequest Request)
    : IRequest<Result<Success>>;
}
