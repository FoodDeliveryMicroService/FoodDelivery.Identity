using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Features.Authentication.Dtos.Login;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.Login;

public sealed record LoginCommand(LoginRequest Request)
    : IRequest<Result<LoginResponse>>;