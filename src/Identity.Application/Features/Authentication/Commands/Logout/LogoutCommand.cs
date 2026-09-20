using Identity.Application.Features.Authentication.Dtos.Logout;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.Logout;

public sealed record LogoutCommand(LogoutRequest Request)
    : IRequest<Result<Success>>;