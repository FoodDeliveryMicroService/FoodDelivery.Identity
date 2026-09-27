using Identity.Application.Features.Authentication.Commands.Login;
using Identity.Application.Features.Authentication.Dtos.Login;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Identity;
using Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.Authentication;

[Collection(WebAppFactoryCollection.CollectionName)]
public class LoginCommandHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<(string email, string password)> SeedConfirmedUserAsync()
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

        return (email, password);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokens()
    {
        var (email, password) = await SeedConfirmedUserAsync();
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new LoginCommand(new LoginRequest(email, password)));

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldFail()
    {
        var (email, _) = await SeedConfirmedUserAsync();
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new LoginCommand(new LoginRequest(email, "WrongPass1!")));

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_WithUnknownUser_ShouldFail()
    {
        var mediator = _factory.CreateMediator();
        var result = await mediator.Send(new LoginCommand(new LoginRequest("none@localhost.test", "Str0ng!Pass")));

        Assert.True(result.IsError);
    }
}