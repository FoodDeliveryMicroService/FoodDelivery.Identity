using Identity.API;
using Identity.Application.Common.Interfaces;
using Identity.Domain.Identity;
using Identity.Infrastructure.Data;
using Identity.Infrastructure.Identity;
using Identity.Tests.Common.Fakes;
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

namespace Identity.Api.IntegrationTests.Common;

/// <summary>
/// Full-stack host: real HTTP pipeline, real SQL Server (via Testcontainers),
/// real JWT auth. Only the outbound email sender is swapped for a fake so
/// tests can read the confirmation code / reset token without a mailbox.
/// See src-addition/IAssemblyMarker.cs for the one file to add to Identity.API.
/// </summary>
public class WebAppFactory : WebApplicationFactory<IAssemblyMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

    public AppHttpClient CreateAppHttpClient() => new(CreateClient());

    public FakeEmailService EmailService => Services.GetRequiredService<FakeEmailService>();
    public FakeGeocodingService GeocodingService => Services.GetRequiredService<FakeGeocodingService>();


    public IAppDbContext CreateAppDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IAppDbContext>();
    }

    /// <summary>
    /// Creates a user through the real UserManager/RoleManager and marks the
    /// email confirmed directly, so tests that only need "a logged-in
    /// customer" don't have to run the full register -> confirm HTTP flow
    /// every time (AuthControllerTests exercises that flow explicitly).
    /// </summary>
    public async Task<AppUser> SeedConfirmedUserAsync(
        string email,
        string password,
        string name,
        string phoneNumber,
        Role role)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!await roleManager.RoleExistsAsync(role.ToString()))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role.ToString()));
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            Name = name,
            PhoneNumber = phoneNumber,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, role.ToString());

        return user;
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

            services.RemoveAll<IEmailService>();
            services.AddSingleton<FakeEmailService>();
            services.AddSingleton<IEmailService>(sp => sp.GetRequiredService<FakeEmailService>());

            services.RemoveAll<IGeocodingService>();
            services.AddSingleton<FakeGeocodingService>();
            services.AddSingleton<IGeocodingService>(sp => sp.GetRequiredService<FakeGeocodingService>());
        });
    }
}
