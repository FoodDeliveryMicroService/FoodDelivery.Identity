using System.Net;
using Identity.Api.IntegrationTests.Common;
using Xunit;

namespace Identity.Api.IntegrationTests.Infrastructure;

[Collection(WebAppFactoryCollection.CollectionName)]
public class HealthCheckTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthy()
    {
        var client = _factory.CreateAppHttpClient();
        var response = await client.GetAsync("health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}