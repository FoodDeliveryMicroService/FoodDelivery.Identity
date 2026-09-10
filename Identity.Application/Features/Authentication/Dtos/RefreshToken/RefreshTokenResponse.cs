using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Features.Authentication.Dtos.RefreshToken;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);