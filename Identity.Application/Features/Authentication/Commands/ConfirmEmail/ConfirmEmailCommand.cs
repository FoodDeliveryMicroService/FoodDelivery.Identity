using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(ConfirmEmailRequest Request)
    : IRequest<Result<Success>>;