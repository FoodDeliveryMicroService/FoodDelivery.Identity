using System.Net;
using Identity.Api.IntegrationTests.Common;
using Identity.Domain.Identity;
using Xunit;

namespace Identity.Api.IntegrationTests.Infrastructure;

[Collection(WebAppFactoryCollection.CollectionName)]
public class AuthorizationTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturn401()
    {
        var client = _factory.CreateAppHttpClient();
        var response = await client.GetAsync("api/profile/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_ShouldReturn401()
    {
        var client = _factory.CreateAppHttpClient();
        client.SetAuthorizationHeader("totally-not-a-jwt");
        var response = await client.GetAsync("api/profile/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminOnlyEndpoint_WithCustomerToken_ShouldReturn403()
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        await _factory.SeedConfirmedUserAsync(email, "Str0ng!Pass", "T", "01000000000", Role.Customer);
        var client = _factory.CreateAppHttpClient();
        var token = await client.LoginAsync(email, "Str0ng!Pass");
        client.SetAuthorizationHeader(token);

        var response = await client.PutAsJsonAsync($"api/usermanagement/{Guid.NewGuid()}/suspend", new {});
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CustomerOnlyEndpoint_WithCustomerToken_ShouldSucceed()
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        await _factory.SeedConfirmedUserAsync(email, "Str0ng!Pass", "T", "01000000000", Role.Customer);
        var client = _factory.CreateAppHttpClient();
        var token = await client.LoginAsync(email, "Str0ng!Pass");
        client.SetAuthorizationHeader(token);

        var response = await client.GetAsync("api/addressmanagement");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}