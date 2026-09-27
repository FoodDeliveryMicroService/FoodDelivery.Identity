using Identity.Application.Features.Profile.Commands.AdminUpdateUser;
using Identity.Application.Features.Profile.Dtos.AdminUpdateUser;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.Profile;

[Collection(WebAppFactoryCollection.CollectionName)]
public class AdminUpdateUserCommandHandlerTests(WebAppFactory factory)
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
    public async Task Handle_ShouldUpdateUserAndCreateAuditLog()
    {
        var userId = await SeedUserAsync();
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new AdminUpdateUserCommand(userId, new AdminUpdateUserRequest("AdminUpdated", "0123456789")));

        Assert.True(result.IsSuccess);

        var context = _factory.CreateAppDbContext();
        var audit = await context.AuditLogs.FirstOrDefaultAsync(a => a.TargetUserId == userId);
        Assert.NotNull(audit);
        Assert.Equal("ProfileUpdatedByAdmin", audit!.Action);
    }

    [Fact]
    public async Task Handle_MissingUser_ShouldFail_And_NotAudit()
    {
        var mediator = _factory.CreateMediator();
        var result = await mediator.Send(new AdminUpdateUserCommand(Guid.NewGuid(), new AdminUpdateUserRequest("X", "1")));

        Assert.True(result.IsError);

        var context = _factory.CreateAppDbContext();
        var anyAudit = await context.AuditLogs.AnyAsync();
        // no need to assert false globally (other tests may add audits), just verify no audit for that ID
        var target = await context.AuditLogs.AnyAsync(a => a.NewValue != null && a.NewValue.Contains("X"));
        Assert.False(target);
    }
}