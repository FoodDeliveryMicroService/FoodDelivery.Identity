using Identity.Application.Features.AddressManagement.Commands.AddAddress;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Application.Features.AddressManagement.Queries.ListAddresses;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Location;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.AddressManagement;

[Collection(WebAppFactoryCollection.CollectionName)]
public class ListMyAddressesQueryHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<(Guid customerId, Governorate gov, City city)> SetupAsync()
    {
        var context = _factory.CreateAppDbContext();
        var unique = Guid.NewGuid().ToString("N")[..8];
        var gov = Governorate.Create(Guid.NewGuid(), $"G_{unique}", $"GE_{unique}").Value;
        var city = City.Create(Guid.NewGuid(), gov.Id, $"C_{unique}", $"CE_{unique}").Value;
        context.Governorates.Add(gov);
        context.Cities.Add(city);
        await context.SaveChangesAsync(default);

        var customerId = await _factory.SeedConfirmedUserAsync();   // ✅
        return (customerId, gov, city);
    }

    private static AddAddressRequest MakeRequest(string label, Governorate g, City c, bool isDefault = false) =>
        new(label, g.Id, c.Id, "St", "1", null, null, null, null, null, isDefault);

    [Fact]
    public async Task Handle_ShouldReturnOnlyCurrentCustomerAddresses()
    {
        var (customerId, gov, city) = await SetupAsync();
        var mediator = _factory.CreateMediator(customerId);

        await mediator.Send(new AddAddressCommand(MakeRequest("Home", gov, city, true)));
        await mediator.Send(new AddAddressCommand(MakeRequest("Work", gov, city)));

        var result = await mediator.Send(new ListAddressesQuery(customerId));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task Handle_ShouldNotReturnOtherCustomerAddresses()
    {
        var (customerA, gov, city) = await SetupAsync();
        var customerB = await _factory.SeedConfirmedUserAsync();   // ✅

        var mediatorA = _factory.CreateMediator(customerA);
        await mediatorA.Send(new AddAddressCommand(MakeRequest("A-Home", gov, city)));

        var mediatorB = _factory.CreateMediator(customerB);
        await mediatorB.Send(new AddAddressCommand(MakeRequest("B-Home", gov, city)));

        var result = await mediatorA.Send(new ListAddressesQuery(customerA));

        Assert.Single(result.Value);
        Assert.Equal("A-Home", result.Value[0].Label);
    }

    [Fact]
    public async Task Handle_WhenNoAddresses_ShouldReturnEmptyList()
    {
        var mediator = _factory.CreateMediator(Guid.NewGuid());
        var result = await mediator.Send(new ListAddressesQuery(Guid.NewGuid()));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_DefaultAddress_ShouldAppearFirst()
    {
        var (customerId, gov, city) = await SetupAsync();
        var mediator = _factory.CreateMediator(customerId);

        await mediator.Send(new AddAddressCommand(MakeRequest("Alpha", gov, city)));
        await mediator.Send(new AddAddressCommand(MakeRequest("Zulu", gov, city, true)));
        await mediator.Send(new AddAddressCommand(MakeRequest("Beta", gov, city)));

        var result = await mediator.Send(new ListAddressesQuery(customerId));

        Assert.Equal("Zulu", result.Value[0].Label);
        Assert.True(result.Value[0].IsDefault);
    }
}