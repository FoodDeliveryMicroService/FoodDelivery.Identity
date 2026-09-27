using Identity.Domain.Email;
using Identity.Tests.Common.Identity;
using Xunit;

namespace Identity.Domain.UnitTests.Email;

public class EmailConfirmationTests
{
    [Fact]
    public void VerifyCode_WithCorrectCode_ShouldReturnTrue()
    {
        var confirmation = EmailConfirmationFactory.CreateEmailConfirmation(code: "654321");

        Assert.True(confirmation.VerifyCode("654321"));
    }

    [Fact]
    public void VerifyCode_WithWrongCode_ShouldReturnFalse()
    {
        var confirmation = EmailConfirmationFactory.CreateEmailConfirmation(code: "654321");

        Assert.False(confirmation.VerifyCode("000000"));
    }

    [Fact]
    public void Use_WhenNotUsedAndNotExpired_ShouldSucceed()
    {
        var confirmation = EmailConfirmationFactory.CreateEmailConfirmation(
            expiresAt: DateTimeOffset.UtcNow.AddMinutes(10));

        var result = confirmation.Use();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void Use_WhenAlreadyUsed_ShouldFail()
    {
        var confirmation = EmailConfirmationFactory.CreateEmailConfirmation();
        confirmation.Use();

        var result = confirmation.Use();

        Assert.True(result.IsError);
        Assert.Equal(EmailConfirmationErrors.InvalidCode.Code, result.TopError.Code);
    }

    [Fact]
    public void Use_WhenExpired_ShouldFail()
    {
        var confirmation = EmailConfirmationFactory.CreateEmailConfirmation(
            expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));

        var result = confirmation.Use();

        Assert.True(result.IsError);
        Assert.Equal(EmailConfirmationErrors.ExpiredCode.Code, result.TopError.Code);
    }

    [Fact]
    public void Use_AfterFiveAttempts_ShouldFailWithTooManyAttempts()
    {
        var confirmation = EmailConfirmationFactory.CreateEmailConfirmation();

        for (var i = 0; i < 5; i++)
        {
            confirmation.IncrementAttempt();
        }

        var result = confirmation.Use();

        Assert.True(result.IsError);
        Assert.Equal(EmailConfirmationErrors.TooManyAttempts.Code, result.TopError.Code);
    }

    [Fact]
    public void IsExpired_WhenPastExpiry_ShouldReturnTrue()
    {
        var confirmation = EmailConfirmationFactory.CreateEmailConfirmation(
            expiresAt: DateTimeOffset.UtcNow.AddMinutes(-5));

        Assert.True(confirmation.IsExpired);
    }
}
