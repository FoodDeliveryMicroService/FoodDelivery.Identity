using Identity.Application.Features.Authentication.Dtos.RefreshToken;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.RefreshToken;

public sealed record RefreshTokenCommand(RefreshTokenRequest Request)
    : IRequest<Result<RefreshTokenResponse>>;