using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Features.Authentication.Dtos.RefreshToken;

public sealed record RefreshTokenRequest(
    string RefreshToken
);
