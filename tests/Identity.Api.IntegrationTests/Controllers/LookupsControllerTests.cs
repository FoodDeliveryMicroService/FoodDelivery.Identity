using System.Net;
using Identity.Api.IntegrationTests.Common;
using Identity.Application.Common.Interfaces;
using Identity.Domain.Location;
using Xunit;

namespace Identity.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class LookupsControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();
    private readonly IAppDbContext _context = webAppFactory.CreateAppDbContext();

    [Fact]
    public async Task GetGovernorates_ShouldReturnOk()
    {
        var response = await _client.GetAsync("api/lookups/governorates");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCities_WithUnknownGovernorate_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"api/lookups/cities/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
