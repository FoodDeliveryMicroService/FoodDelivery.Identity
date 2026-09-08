using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Identity.Application.Features.Authentication.Dtos.RegisterUser;
using Identity.Domain.Common.Results;

namespace Identity.Application.Features.Authentication.Commands.RegisterUser
{
    public sealed record RegisterUserCommand(RegisterUserRequest Request)
    : IRequest<Result<RegisterUserResponse>>;

}
