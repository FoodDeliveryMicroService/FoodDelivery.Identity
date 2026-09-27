using System.Net;
using Identity.Api.IntegrationTests.Common;
using Identity.Application.Features.Profile.Dtos.UpdateProfile;
using Identity.Domain.Identity;
using Xunit;

namespace Identity.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class ProfileControllerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<AppHttpClient> AuthedClientAsync(Role role = Role.Customer)
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        await _factory.SeedConfirmedUserAsync(email, "Str0ng!Pass", "T", "01000000000", role);
        var client = _factory.CreateAppHttpClient();
        var token = await client.LoginAsync(email, "Str0ng!Pass");
        client.SetAuthorizationHeader(token);
        return client;
    }

    [Fact]
    public async Task GetMyProfile_WithoutAuth_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateAppHttpClient();
        var response = await client.GetAsync("api/profile/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyProfile_Authenticated_ShouldReturnOk()
    {
        var client = await AuthedClientAsync();
        var response = await client.GetAsync("api/profile/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMyProfile_Authenticated_ShouldReturnOk()
    {
        var client = await AuthedClientAsync();
        var response = await client.PutAsJsonAsync("api/profile/me", new UpdateProfileRequest("New", "01000000001"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminUpdate_AsCustomer_ShouldReturnForbidden()
    {
        var client = await AuthedClientAsync(Role.Customer);
        var response = await client.PutAsJsonAsync(
            $"api/profile/{Guid.NewGuid()}/admin",
            new { Name = "X", PhoneNumber = "1" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}