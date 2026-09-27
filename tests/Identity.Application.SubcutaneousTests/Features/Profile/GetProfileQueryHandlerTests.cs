using Identity.Application.Features.Profile.Queries.GetProfile;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Identity;
using Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.Profile;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetProfileQueryHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<Guid> SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!await roleManager.RoleExistsAsync("Customer"))
            await roleManager.CreateAsync(new IdentityRole<Guid>("Customer"));

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = $"{Guid.NewGuid()}@localhost.test",
            Email = $"{Guid.NewGuid()}@localhost.test",
            Name = "Test",
            PhoneNumber = "01000000000",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, "Str0ng!Pass");
        await userManager.AddToRoleAsync(user, "Customer");
        return user.Id;
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldReturnProfile()
    {
        var userId = await SeedUserAsync();
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new GetProfileQuery(userId));

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Contains("Customer", result.Value.Roles);
    }

    [Fact]
    public async Task Handle_MissingUser_ShouldFail()
    {
        var mediator = _factory.CreateMediator();
        var result = await mediator.Send(new GetProfileQuery(Guid.NewGuid()));

        Assert.True(result.IsError);
    }
}