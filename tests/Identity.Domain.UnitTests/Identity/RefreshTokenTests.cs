using Identity.Domain.Identity.Errors;
using Identity.Tests.Common.Identity;
using Xunit;

namespace Identity.Domain.UnitTests.Identity;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var expiresOnUtc = DateTimeOffset.UtcNow.AddDays(7);

        var result = RefreshTokenFactory.CreateRefreshToken(id: id, token: "abc", userId: userId, expiresOnUtc: expiresOnUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal("abc", result.Value.Token);
        Assert.Equal(userId, result.Value.UserId);
        Assert.False(result.Value.IsRevoked);
    }

    [Fact]
    public void Create_WithEmptyId_ShouldFail()
    {
        var result = RefreshTokenFactory.CreateRefreshToken(id: Guid.Empty);

        Assert.True(result.IsError);
        Assert.Equal(RefreshTokenErrors.IdRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidToken_ShouldFail(string token)
    {
        var result = RefreshTokenFactory.CreateRefreshToken(token: token);

        Assert.True(result.IsError);
        Assert.Equal(RefreshTokenErrors.TokenRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_WithPastExpiry_ShouldFail()
    {
        var result = RefreshTokenFactory.CreateRefreshToken(expiresOnUtc: DateTimeOffset.UtcNow.AddMinutes(-1));

        Assert.True(result.IsError);
        Assert.Equal(RefreshTokenErrors.ExpiryInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Revoke_WhenNotRevoked_ShouldSucceed()
    {
        var token = RefreshTokenFactory.CreateRefreshToken().Value;

        var result = token.Revoke();

        Assert.True(result.IsSuccess);
        Assert.True(token.IsRevoked);
        Assert.NotNull(token.RevokedOnUtc);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldFail()
    {
        var token = RefreshTokenFactory.CreateRefreshToken().Value;
        token.Revoke();

        var result = token.Revoke();

        Assert.True(result.IsError);
        Assert.Equal(RefreshTokenErrors.AlreadyRevoked.Code, result.TopError.Code);
    }
}
