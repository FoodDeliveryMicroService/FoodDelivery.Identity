using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Features.Authentication.Dtos.Email
{
    public sealed record ConfirmEmailRequest(string Email, string Code);

}
