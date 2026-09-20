using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Features.Profile.Dtos.AdminUpdateUser;

public sealed record AdminUpdateUserRequest(string? Name, string? PhoneNumber);