using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(ConfirmEmailRequest Request)
    : IRequest<Result<Success>>;