using Identity.Application.Features.Authentication.Dtos.ResetPassword;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.ResetPassword;

public sealed record ResetPasswordCommand(ResetPasswordRequest Request)
    : IRequest<Result<Success>>;