using Identity.Application.Features.Authentication.Dtos.ResetPassword;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(RequestPasswordResetRequest Request)
    : IRequest<Result<Success>>;