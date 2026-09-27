using Identity.Application.Features.GeographicLookup.Queries.ListGovernorates;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Location;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.GeographicLookup;

[Collection(WebAppFactoryCollection.CollectionName)]
public class ListGovernoratesQueryHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    [Fact]
    public async Task Handle_ShouldReturnSeededGovernorate()
    {
        var context = _factory.CreateAppDbContext();
        var gov = Governorate.Create(Guid.NewGuid(), $"ÏãíÇØ_{Guid.NewGuid():N}"[..20], $"Damietta_{Guid.NewGuid():N}"[..20]).Value;
        context.Governorates.Add(gov);
        await context.SaveChangesAsync(default);

        var mediator = _factory.CreateMediator();
        var result = await mediator.Send(new ListGovernoratesQuery());

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value, g => g.Id == gov.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnDeterministicOrder()
    {
        var mediator = _factory.CreateMediator();

        var first = await mediator.Send(new ListGovernoratesQuery());
        var second = await mediator.Send(new ListGovernoratesQuery());

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(
            first.Value.Select(g => g.Id).ToList(),
            second.Value.Select(g => g.Id).ToList());
    }
}