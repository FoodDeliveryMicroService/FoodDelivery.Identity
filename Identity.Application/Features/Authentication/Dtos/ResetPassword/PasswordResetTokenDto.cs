using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Features.Authentication.Dtos.ResetPassword;

public sealed record PasswordResetTokenDto(string Email, string UserName, string Token);