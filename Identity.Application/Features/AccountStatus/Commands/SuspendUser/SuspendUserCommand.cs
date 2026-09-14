using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Features.AccountStatus.Dtos;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.AccountStatus.Commands.SuspendUser;

public sealed record SuspendUserCommand(Guid UserId) : IRequest<Result<AccountStatusResponse>>;