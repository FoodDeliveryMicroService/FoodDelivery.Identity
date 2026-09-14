using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Features.Profile.Dtos.UpdateProfile;

public sealed record UpdateProfileRequest(string? Name, string? PhoneNumber);