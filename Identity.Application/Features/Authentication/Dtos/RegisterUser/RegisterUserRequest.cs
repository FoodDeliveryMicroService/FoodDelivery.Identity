using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain.Identity;

namespace Identity.Application.Features.Authentication.Dtos.RegisterUser
{
    public sealed record RegisterUserRequest(
        string Name, string Email, string PhoneNumber, string Password,Role Role);
}
