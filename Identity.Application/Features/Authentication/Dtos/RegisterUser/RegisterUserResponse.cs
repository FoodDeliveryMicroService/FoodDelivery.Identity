using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Features.Authentication.Dtos.RegisterUser
{
    public sealed record RegisterUserResponse(Guid UserId, string Email, string Name);

}
