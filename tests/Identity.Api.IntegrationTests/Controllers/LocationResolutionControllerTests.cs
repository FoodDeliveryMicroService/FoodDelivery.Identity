using System.Net;
using Identity.Api.IntegrationTests.Common;
using Identity.Tests.Common.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class LocationResolutionControllerTests(WebAppFactory webAppFactory)
{
    private readonly WebAppFactory _factory = webAppFactory;

    [Fact]
    public async Task Resolve_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateAppHttpClient();
        var response = await client.GetAsync("api/locationresolution/resolve?latitude=30&longitude=31");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Resolve_WithInvalidCoordinates_ShouldReturnBadRequest()
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        await _factory.SeedConfirmedUserAsync(email, "Str0ng!Pass", "T", "01000000000", Identity.Domain.Identity.Role.Customer);
        var client = _factory.CreateAppHttpClient();
        var token = await client.LoginAsync(email, "Str0ng!Pass");
        client.SetAuthorizationHeader(token);

        var response = await client.GetAsync("api/locationresolution/resolve?latitude=999&longitude=999");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}