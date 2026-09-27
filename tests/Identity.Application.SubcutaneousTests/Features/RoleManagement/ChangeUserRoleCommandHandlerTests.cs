using Identity.Application.Features.RoleManagement.Commands.ChangeUserRole;
using Identity.Application.Features.RoleManagement.Dtos.ChangeUserRole;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Identity;
using Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.RoleManagement;

[Collection(WebAppFactoryCollection.CollectionName)]
public class ChangeUserRoleCommandHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<Guid> SeedUserAsync(Role role = Role.Customer)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!await roleManager.RoleExistsAsync(role.ToString()))
            await roleManager.CreateAsync(new IdentityRole<Guid>(role.ToString()));
        if (!await roleManager.RoleExistsAsync(Role.RestaurantOwner.ToString()))
            await roleManager.CreateAsync(new IdentityRole<Guid>(Role.RestaurantOwner.ToString()));

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
        await userManager.AddToRoleAsync(user, role.ToString());
        return user.Id;
    }

    [Fact]
    public async Task Handle_ValidChange_ShouldSucceedAndAudit()
    {
        var userId = await SeedUserAsync(Role.Customer);
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new ChangeUserRoleCommand(userId, new ChangeUserRoleRequest(Role.RestaurantOwner)));

        Assert.True(result.IsSuccess);

        var context = _factory.CreateAppDbContext();
        var audit = await context.AuditLogs.FirstOrDefaultAsync(a => a.TargetUserId == userId);
        Assert.NotNull(audit);
        Assert.Equal("RoleChanged", audit!.Action);
    }

    [Fact]
    public async Task Handle_MissingUser_ShouldFail()
    {
        var mediator = _factory.CreateMediator();
        var result = await mediator.Send(new ChangeUserRoleCommand(Guid.NewGuid(), new ChangeUserRoleRequest(Role.Customer)));

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_SameRole_ShouldFail()
    {
        var userId = await SeedUserAsync(Role.Customer);
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new ChangeUserRoleCommand(userId, new ChangeUserRoleRequest(Role.Customer)));

        Assert.True(result.IsError);
    }
}