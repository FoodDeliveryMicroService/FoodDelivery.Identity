using MediatR;
using Identity.Application.Features.Authentication.Dtos.RegisterUser;
using Identity.Domain.Common.Results;

namespace Identity.Application.Features.Authentication.Commands.RegisterUser
{
    public sealed record RegisterUserCommand(RegisterUserRequest Request)
    : IRequest<Result<RegisterUserResponse>>;

}
