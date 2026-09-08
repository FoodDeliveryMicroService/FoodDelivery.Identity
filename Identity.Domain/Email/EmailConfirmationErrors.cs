using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain.Common.Results;

namespace Identity.Domain.Email
{
    public static class EmailConfirmationErrors
    {
        public static Error InvalidCode =>
            Error.Validation("Email.InvalidCode", "The confirmation code is invalid.");

        public static Error ExpiredCode =>
            Error.Validation("Email.ExpiredCode", "The confirmation code has expired.");

        public static Error AlreadyConfirmed =>
            Error.Conflict("Email.AlreadyConfirmed", "This email is already confirmed.");

        public static Error CodeGenerationFailed =>
            Error.Failure("Email.CodeGenerationFailed", "Failed to generate confirmation code.");

        public static Error TooManyAttempts =>
            Error.TooManyRequests("Email.TooManyAttempts", "Too many attempts. Please wait 5 minutes.");
    }
}
