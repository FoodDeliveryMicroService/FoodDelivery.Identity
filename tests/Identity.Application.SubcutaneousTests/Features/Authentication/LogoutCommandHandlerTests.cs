using Identity.Application.Features.Authentication.Commands.Login;
using Identity.Application.Features.Authentication.Commands.Logout;
using Identity.Application.Features.Authentication.Commands.RefreshToken;
using Identity.Application.Features.Authentication.Dtos.Login;
using Identity.Application.Features.Authentication.Dtos.Logout;
using Identity.Application.Features.Authentication.Dtos.RefreshToken;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Identity;
using Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.Authentication;

[Collection(WebAppFactoryCollection.CollectionName)]
public class LogoutCommandHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<(Guid userId, string refreshToken)> SetupAsync()
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        const string password = "Str0ng!Pass";

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!await roleManager.RoleExistsAsync("Customer"))
            await roleManager.CreateAsync(new IdentityRole<Guid>("Customer"));

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            Name = "Test",
            PhoneNumber = "01000000000",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, "Customer");

        var mediator = _factory.CreateMediator();
        var login = await mediator.Send(new LoginCommand(new LoginRequest(email, password)));
        return (user.Id, login.Value.RefreshToken);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldRevokeSuccessfully()
    {
        var (userId, refresh) = await SetupAsync();
        var mediator = _factory.CreateMediator(userId);

        var result = await mediator.Send(new LogoutCommand(new LogoutRequest(refresh)));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_AfterLogout_RefreshShouldFail()
    {
        var (userId, refresh) = await SetupAsync();
        var mediator = _factory.CreateMediator(userId);

        await mediator.Send(new LogoutCommand(new LogoutRequest(refresh)));
        var refreshResult = await mediator.Send(new RefreshTokenCommand(new RefreshTokenRequest(refresh)));

        Assert.True(refreshResult.IsError);
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldFail()
    {
        var (userId, _) = await SetupAsync();
        var mediator = _factory.CreateMediator(userId);

        var result = await mediator.Send(new LogoutCommand(new LogoutRequest("bogus")));

        Assert.True(result.IsError);
    }
}