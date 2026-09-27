using Identity.Application.Features.Authentication.Commands.ConfirmEmail;
using Identity.Application.Features.Authentication.Commands.Login;
using Identity.Application.Features.Authentication.Commands.RegisterUser;
using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Application.Features.Authentication.Dtos.Login;
using Identity.Application.Features.Authentication.Dtos.RegisterUser;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Identity;
using MediatR;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.Authentication;

[Collection(WebAppFactoryCollection.CollectionName)]
public class ConfirmEmailCommandHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<(Guid UserId, string Email, string Password, IMediator Mediator)> RegisterUnconfirmedUserAsync()
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        const string password = "Str0ng!Pass";
        var mediator = _factory.CreateMediator();

        var registerResult = await mediator.Send(
            new RegisterUserCommand(new RegisterUserRequest("Donia", email, "01000000000", password, Role.Customer)));

        return (registerResult.Value.UserId, email, password, mediator);
    }

    [Fact]
    public async Task Handle_WithCorrectCode_ShouldConfirmEmailAndAllowLoginAfterward()
    {
        var (userId, email, password, mediator) = await RegisterUnconfirmedUserAsync();
        var code = _factory.EmailService.LastConfirmationCode!;

        var confirmResult = await mediator.Send(new ConfirmEmailCommand(new ConfirmEmailRequest(userId.ToString(), code)));
        Assert.True(confirmResult.IsSuccess);

        var loginResult = await mediator.Send(new LoginCommand(new LoginRequest(email, password)));
        Assert.True(loginResult.IsSuccess);
    }

    [Fact]
    public async Task Handle_WithWrongCode_ShouldFail()
    {
        var (userId, _, _, mediator) = await RegisterUnconfirmedUserAsync();

        var result = await mediator.Send(new ConfirmEmailCommand(new ConfirmEmailRequest(userId.ToString(), "000000")));

        Assert.True(result.IsError);
        Assert.Equal("Email.InvalidCode", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WithAlreadyUsedCode_ShouldFailOnSecondAttempt()
    {
        var (userId, _, _, mediator) = await RegisterUnconfirmedUserAsync();
        var code = _factory.EmailService.LastConfirmationCode!;

        await mediator.Send(new ConfirmEmailCommand(new ConfirmEmailRequest(userId.ToString(), code)));
        var secondAttempt = await mediator.Send(new ConfirmEmailCommand(new ConfirmEmailRequest(userId.ToString(), code)));

        Assert.True(secondAttempt.IsError);
        Assert.Equal("Email.AlreadyConfirmed", secondAttempt.TopError.Code);
    }

    [Fact]
    public async Task Handle_ForNonExistingUser_ShouldFail()
    {
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new ConfirmEmailCommand(new ConfirmEmailRequest(Guid.NewGuid().ToString(), "123456")));

        Assert.True(result.IsError);
    }
}