using Identity.Application.Common.Interfaces;

namespace Identity.Tests.Common.Security;

/// <summary>
/// Test double for IUser. The real CurrentUser reads the id from
/// HttpContext claims, which doesn't exist when calling MediatR directly
/// in Subcutaneous tests. Register this in place of IUser and set the Id
/// per test/scope to simulate "who is calling".
/// </summary>
public class TestCurrentUser : IUser
{
    public Guid? Id { get; set; }
}
