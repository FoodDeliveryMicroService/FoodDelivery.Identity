using Identity.Application.Features.Authentication.Commands.RegisterUser;
using Identity.Application.Features.Authentication.Dtos.RegisterUser;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Identity;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.Authentication;

/// <summary>
/// Runs through the real IIdentityService (ASP.NET Identity + UserManager +
/// SQL Server) and the real MediatR pipeline (Register -> SendConfirmationCode),
/// with only the outbound email swapped for FakeEmailService. This is exactly
/// the boundary the checklist calls out: "Application -> IIdentityService ->
/// ASP.NET Identity -> SQL Server actually works".
/// </summary>
[Collection(WebAppFactoryCollection.CollectionName)]
public class RegisterUserCommandHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    [Fact]
    public async Task Handle_WithUniqueEmail_ShouldCreateUserAndSendConfirmationCode()
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        var command = new RegisterUserCommand(new RegisterUserRequest("Donia", email, "01000000000", "Str0ng!Pass", Role.Customer));

        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.Equal(email, result.Value.Email);
        Assert.NotNull(_factory.EmailService.LastConfirmationCode);
        Assert.Equal(6, _factory.EmailService.LastConfirmationCode!.Length);
    }

    [Fact]
    public async Task Handle_WithAlreadyRegisteredEmail_ShouldReturnConflict()
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        var command = new RegisterUserCommand(new RegisterUserRequest("Donia", email, "01000000000", "Str0ng!Pass", Role.Customer));

        var mediator = _factory.CreateMediator();
        await mediator.Send(command);

        var duplicateResult = await mediator.Send(command);

        Assert.True(duplicateResult.IsError);
        Assert.Equal("DuplicateEmail", duplicateResult.TopError.Code);
    }

    [Fact]
    public async Task Handle_NewlyRegisteredUser_ShouldNotBeAbleToLoginBeforeConfirmingEmail()
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        const string password = "Str0ng!Pass";
        var mediator = _factory.CreateMediator();

        await mediator.Send(new RegisterUserCommand(new RegisterUserRequest("Donia", email, "01000000000", password, Role.Customer)));

        var loginResult = await mediator.Send(
            new Identity.Application.Features.Authentication.Commands.Login.LoginCommand(
                new Identity.Application.Features.Authentication.Dtos.Login.LoginRequest(email, password)));

        Assert.True(loginResult.IsError);
        Assert.Equal("Auth_AccountNotConfirmed", loginResult.TopError.Code);
    }
}