using System.Net;
using Identity.Api.IntegrationTests.Common;
using Xunit;

namespace Identity.Api.IntegrationTests.Infrastructure;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RateLimitingTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    [Fact]
    public async Task Requests_UnderLimit_ShouldNotReturn429()
    {
        var client = _factory.CreateAppHttpClient();

        for (var i = 0; i < 5; i++)
        {
            var response = await client.GetAsync("api/lookups/governorates");
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }
    }
}