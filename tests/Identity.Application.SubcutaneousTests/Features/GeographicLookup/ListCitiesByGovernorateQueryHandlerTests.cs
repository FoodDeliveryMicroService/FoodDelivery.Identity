using Identity.Application.Features.GeographicLookup.Queries.ListCitiesByGovernorate;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Location;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.GeographicLookup;

[Collection(WebAppFactoryCollection.CollectionName)]
public class ListCitiesByGovernorateQueryHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<(Governorate gov1, Governorate gov2, City city1, City city2)> SetupAsync()
    {
        var context = _factory.CreateAppDbContext();
        var unique = Guid.NewGuid().ToString("N")[..8];
        var gov1 = Governorate.Create(Guid.NewGuid(), $"G1_{unique}", $"GE1_{unique}").Value;
        var gov2 = Governorate.Create(Guid.NewGuid(), $"G2_{unique}", $"GE2_{unique}").Value;
        var city1 = City.Create(Guid.NewGuid(), gov1.Id, $"C1_{unique}", $"CE1_{unique}").Value;
        var city2 = City.Create(Guid.NewGuid(), gov2.Id, $"C2_{unique}", $"CE2_{unique}").Value;
        context.Governorates.AddRange(gov1, gov2);
        context.Cities.AddRange(city1, city2);
        await context.SaveChangesAsync(default);
        return (gov1, gov2, city1, city2);
    }

    [Fact]
    public async Task Handle_WithExistingGovernorate_ShouldReturnItsCities()
    {
        var (gov1, _, city1, _) = await SetupAsync();
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new ListCitiesByGovernorateQuery(gov1.Id));

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value, c => c.Id == city1.Id);
    }

    [Fact]
    public async Task Handle_ShouldNotReturnCitiesFromOtherGovernorates()
    {
        var (gov1, _, _, city2) = await SetupAsync();
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new ListCitiesByGovernorateQuery(gov1.Id));

        Assert.DoesNotContain(result.Value, c => c.Id == city2.Id);
    }

    [Fact]
    public async Task Handle_WithUnknownGovernorate_ShouldReturnNotFound()
    {
        var mediator = _factory.CreateMediator();
        var result = await mediator.Send(new ListCitiesByGovernorateQuery(Guid.NewGuid()));

        Assert.True(result.IsError);
        Assert.Equal("Governorate.NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_GovernorateWithNoCities_ShouldReturnEmpty()
    {
        var context = _factory.CreateAppDbContext();
        var unique = Guid.NewGuid().ToString("N")[..8];
        var gov = Governorate.Create(Guid.NewGuid(), $"G_{unique}", $"GE_{unique}").Value;
        context.Governorates.Add(gov);
        await context.SaveChangesAsync(default);

        var mediator = _factory.CreateMediator();
        var result = await mediator.Send(new ListCitiesByGovernorateQuery(gov.Id));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}