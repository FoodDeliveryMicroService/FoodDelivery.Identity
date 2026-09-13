using System.Security.Cryptography;
using System.Text;
using Identity.Domain.Common.Results;

namespace Identity.Domain.Email
{
    public sealed class EmailConfirmation
    {
        public Guid Id { get; private set; }
        public string UserId { get; private set; }
        public string CodeHash { get; private set; } // renamed: this now stores a hash, not the raw code
        public DateTimeOffset ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public int AttemptCount { get; private set; }

        private EmailConfirmation() { } // For EF Core

        // Single entry point for creation — no more duplicate constructor/factory
        public EmailConfirmation(string userId, string code, DateTimeOffset expiresAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            CodeHash = HashCode(code);
            ExpiresAt = expiresAt;
            IsUsed = false;
            CreatedAt = DateTimeOffset.UtcNow;
            AttemptCount = 0;
        }

        public bool VerifyCode(string code)
        {
            var incomingHash = HashCode(code);
            // Fixed-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(incomingHash),
                Encoding.UTF8.GetBytes(CodeHash));
        }

        private static string HashCode(string code) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

        public Result<bool> Use()
        {
            if (IsUsed)
                return EmailConfirmationErrors.InvalidCode;

            if (ExpiresAt < DateTimeOffset.UtcNow)
                return EmailConfirmationErrors.ExpiredCode;

            if (AttemptCount >= 5)
                return EmailConfirmationErrors.TooManyAttempts;

            IsUsed = true;
            return true;
        }


        public void IncrementAttempt() => AttemptCount++;

        public bool IsExpired => ExpiresAt < DateTimeOffset.UtcNow;
    }
}