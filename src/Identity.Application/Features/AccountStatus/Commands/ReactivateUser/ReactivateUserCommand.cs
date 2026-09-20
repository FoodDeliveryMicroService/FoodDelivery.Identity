using Identity.Application.Features.AccountStatus.Dtos;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.AccountStatus.Commands.ReactivateUser;

public sealed record ReactivateUserCommand(Guid UserId) : IRequest<Result<AccountStatusResponse>>;