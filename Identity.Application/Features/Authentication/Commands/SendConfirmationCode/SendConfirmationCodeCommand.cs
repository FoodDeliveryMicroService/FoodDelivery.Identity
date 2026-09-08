using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.SendConfirmationCode
{
    public sealed record SendConfirmationCodeCommand(SendConfirmationCodeRequest Request)
    : IRequest<Result<Success>>;
}
