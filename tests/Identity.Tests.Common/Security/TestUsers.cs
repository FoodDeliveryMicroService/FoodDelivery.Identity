namespace Identity.Tests.Common.Security;

/// <summary>
/// Canonical set of test account credentials used across Subcutaneous and
/// Api integration tests. Each test that needs a real signed-in user should
/// seed one of these (or a unique email per test) via
/// WebAppFactory.SeedConfirmedUserAsync rather than relying on shared state.
/// </summary>
public static class TestUsers
{
    public static class Customer01
    {
        public const string Email = "customer01@localhost.test";
        public const string Password = "Str0ng!Pass";
        public const string Name = "Customer One";
        public const string PhoneNumber = "01000000001";
    }

    public static class Admin01
    {
        public const string Email = "admin01@localhost.test";
        public const string Password = "Str0ng!Pass";
        public const string Name = "Admin One";
        public const string PhoneNumber = "01000000002";
    }
}
