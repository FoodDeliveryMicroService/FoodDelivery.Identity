using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain.Common.Results;

namespace Identity.Domain.Email
{
    public sealed class EmailConfirmation
    {
        public Guid Id { get; private set; }
        public string UserId { get; private set; }
        public string Code { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public int AttemptCount { get; private set; }

        private EmailConfirmation() { } // For EF Core

        public EmailConfirmation(string userId, string code, DateTime expiresAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Code = code;
            ExpiresAt = expiresAt;
            IsUsed = false;
            CreatedAt = DateTime.UtcNow;
            AttemptCount = 0;
        }

        public Result<bool> Use()
        {
            if (IsUsed)
                return EmailConfirmationErrors.InvalidCode;

            if (ExpiresAt < DateTime.UtcNow)
                return EmailConfirmationErrors.ExpiredCode;

            if (AttemptCount >= 5)
                return EmailConfirmationErrors.TooManyAttempts;

            IsUsed = true;
            return true;
        }

        public void IncrementAttempt()
        {
            AttemptCount++;
        }

        public bool IsExpired => ExpiresAt < DateTime.UtcNow;
    }
}
