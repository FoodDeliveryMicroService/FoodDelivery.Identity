using System.Net;
using System.Net.Http.Json;
using Identity.Api.IntegrationTests.Common;
using Identity.Domain.Identity;
using Xunit;

namespace Identity.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RoleManagementControllerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    [Fact]
    public async Task ChangeUserRole_AsAnonymous_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateAppHttpClient();
        var response = await client.PutAsJsonAsync($"api/usermanagement/{Guid.NewGuid()}/role", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeUserRole_AsCustomer_ShouldReturnForbidden()
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        await _factory.SeedConfirmedUserAsync(email, "Str0ng!Pass", "T", "01000000000", Role.Customer);
        var client = _factory.CreateAppHttpClient();
        var token = await client.LoginAsync(email, "Str0ng!Pass");
        client.SetAuthorizationHeader(token);

        using var form = new MultipartFormDataContent
        {
            { new StringContent("RestaurantOwner"), "NewRole" }
        };

        var response = await client.SendPutFormAsync($"api/usermanagement/{Guid.NewGuid()}/role", form);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ChangeUserRole_AsAdmin_ShouldReturnOkOrNotFound()
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        await _factory.SeedConfirmedUserAsync(email, "Str0ng!Pass", "Admin", "01000000000", Role.Admin);
        var client = _factory.CreateAppHttpClient();
        var token = await client.LoginAsync(email, "Str0ng!Pass");
        client.SetAuthorizationHeader(token);

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Customer"), "NewRole" }
        };

        // Use a bogus userId — expect NotFound, but importantly NOT 401/403.
        var response = await client.SendFormAsync($"api/usermanagement/{Guid.NewGuid()}/role", form);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SuspendUser_AsAdmin_ShouldSucceedOrReturnNotFound()
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        var admin = await _factory.SeedConfirmedUserAsync(email, "Str0ng!Pass", "Admin", "01000000000", Role.Admin);
        var client = _factory.CreateAppHttpClient();
        var token = await client.LoginAsync(email, "Str0ng!Pass");
        client.SetAuthorizationHeader(token);

        var response = await client.PutAsJsonAsync($"api/usermanagement/{Guid.NewGuid()}/suspend", new { });
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}