using Identity.Application.Features.Profile.Commands.UpdateProfile;
using Identity.Application.Features.Profile.Dtos.UpdateProfile;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.Profile;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateProfileCommandHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<Guid> SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = $"{Guid.NewGuid()}@localhost.test",
            Email = $"{Guid.NewGuid()}@localhost.test",
            Name = "Original",
            PhoneNumber = "01000000000",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, "Str0ng!Pass");
        return user.Id;
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldUpdateProfile()
    {
        var userId = await SeedUserAsync();
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new UpdateProfileCommand(userId, new UpdateProfileRequest("Updated", "01111111111")));

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", result.Value.Name);
        Assert.Equal("01111111111", result.Value.PhoneNumber);
    }

    [Fact]
    public async Task Handle_MissingUser_ShouldFail()
    {
        var mediator = _factory.CreateMediator();
        var result = await mediator.Send(new UpdateProfileCommand(Guid.NewGuid(), new UpdateProfileRequest("X", "1")));

        Assert.True(result.IsError);
    }
}