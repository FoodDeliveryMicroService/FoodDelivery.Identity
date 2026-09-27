using Identity.Application.Features.Authentication.Commands.Login;
using Identity.Application.Features.Authentication.Commands.RefreshToken;
using Identity.Application.Features.Authentication.Dtos.Login;
using Identity.Application.Features.Authentication.Dtos.RefreshToken;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Identity;
using Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.Authentication;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RefreshTokenCommandHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<string> LoginAndGetRefreshTokenAsync()
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
        return login.Value.RefreshToken;
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldReturnNewTokens()
    {
        var refresh = await LoginAndGetRefreshTokenAsync();
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new RefreshTokenCommand(new RefreshTokenRequest(refresh)));

        Assert.True(result.IsSuccess);
        Assert.NotEqual(refresh, result.Value.RefreshToken);
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldFail()
    {
        var mediator = _factory.CreateMediator();
        var result = await mediator.Send(new RefreshTokenCommand(new RefreshTokenRequest("bogus")));

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_AfterRotation_OldTokenShouldBeRevoked()
    {
        var refresh = await LoginAndGetRefreshTokenAsync();
        var mediator = _factory.CreateMediator();

        await mediator.Send(new RefreshTokenCommand(new RefreshTokenRequest(refresh)));
        var result = await mediator.Send(new RefreshTokenCommand(new RefreshTokenRequest(refresh)));

        Assert.True(result.IsError);
    }
}