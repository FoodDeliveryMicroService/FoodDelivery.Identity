using Identity.API;
using Identity.Application.Common.Interfaces;
using Identity.Domain.Identity;
using Identity.Infrastructure.Data;
using Identity.Infrastructure.Identity;
using Identity.Tests.Common.Fakes;
using Identity.Tests.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Common;

/// <summary>
/// Boots the real Identity.API host (DI graph, MediatR pipeline, EF Core
/// configuration) against a disposable SQL Server container, but bypasses
/// HTTP: tests resolve IMediator/IAppDbContext straight from the container.
/// This is the "subcutaneous" layer between pure unit tests and full HTTP
/// integration tests. See src-addition/IAssemblyMarker.cs for the one file
/// you need to add to Identity.API for WebApplicationFactory to work.
/// </summary>
public class WebAppFactory : WebApplicationFactory<IAssemblyMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

    public FakeEmailService EmailService => Services.GetRequiredService<FakeEmailService>();
    public FakeGeocodingService GeocodingService => Services.GetRequiredService<FakeGeocodingService>();


    /// <summary>
    /// Resolves IMediator from a fresh DI scope. Pass currentUserId to
    /// simulate the authenticated customer/admin that commands like
    /// AddAddressCommand read via IUser.
    /// </summary>
    public IMediator CreateMediator(Guid? currentUserId = null)
    {
        var scope = Services.CreateScope();

        if (currentUserId is not null)
        {
            var user = (TestCurrentUser)scope.ServiceProvider.GetRequiredService<IUser>();
            user.Id = currentUserId;
        }

        return scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    public IAppDbContext CreateAppDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IAppDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_dbContainer.GetConnectionString());

        await using var context = new AppDbContext(optionsBuilder.Options);
        await context.Database.MigrateAsync();
    }

    public new Task DisposeAsync() => _dbContainer.StopAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });

            // No HttpContext exists when calling MediatR directly, so swap
            // the real claims-based CurrentUser for a settable test double.
            services.RemoveAll<IUser>();
            services.AddScoped<IUser, TestCurrentUser>();

            // Never hit real SMTP / wwwroot templates from tests.
            services.RemoveAll<IEmailService>();
            services.AddSingleton<FakeEmailService>();
            services.AddSingleton<IEmailService>(sp => sp.GetRequiredService<FakeEmailService>());

            services.RemoveAll<IGeocodingService>();
            services.AddSingleton<FakeGeocodingService>();
            services.AddSingleton<IGeocodingService>(sp => sp.GetRequiredService<FakeGeocodingService>());
        });
    }

    public async Task<Guid> SeedConfirmedUserAsync(
        string? email = null,
        string password = "Str0ng!Pass",
        Role role = Role.Customer)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!await roleManager.RoleExistsAsync(role.ToString()))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role.ToString()));
        }

        var unique = Guid.NewGuid().ToString("N")[..8];
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email ?? $"user_{unique}@localhost.test",
            Email = email ?? $"user_{unique}@localhost.test",
            Name = "Test Customer",
            PhoneNumber = "01000000000",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, role.ToString());
        return user.Id;
    }
}

