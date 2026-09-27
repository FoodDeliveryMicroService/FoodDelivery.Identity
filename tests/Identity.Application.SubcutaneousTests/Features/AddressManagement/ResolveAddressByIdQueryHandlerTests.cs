using Identity.Application.Features.AddressManagement.Commands.AddAddress;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Application.Features.AddressManagement.Queries.ResolveAddressById;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Location;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.AddressManagement;

[Collection(WebAppFactoryCollection.CollectionName)]
public class ResolveAddressByIdQueryHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<(Guid customerId, Guid addressId)> SeedAsync()
    {
        var context = _factory.CreateAppDbContext();
        var unique = Guid.NewGuid().ToString("N")[..8];
        var gov = Governorate.Create(Guid.NewGuid(), $"GR_{unique}", $"GRE_{unique}").Value;
        var city = City.Create(Guid.NewGuid(), gov.Id, $"CR_{unique}", $"CRE_{unique}").Value;
        context.Governorates.Add(gov);
        context.Cities.Add(city);
        await context.SaveChangesAsync(default);

        var customerId = await _factory.SeedConfirmedUserAsync();   // ✅
        var mediator = _factory.CreateMediator(customerId);

        var added = await mediator.Send(new AddAddressCommand(
            new AddAddressRequest("Home", gov.Id, city.Id, "St", "1", null, null, null, null, null, false)));

        return (customerId, added.Value.Id);
    }

    [Fact]
    public async Task Handle_OwnedAddress_ShouldReturnFullDetails()
    {
        var (customerId, addressId) = await SeedAsync();
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new ResolveAddressByIdQuery(addressId, customerId));

        Assert.True(result.IsSuccess);
        Assert.Equal(addressId, result.Value.Id);
    }

    [Fact]
    public async Task Handle_AddressDoesNotExist_ShouldReturnNotFound()
    {
        var mediator = _factory.CreateMediator();
        var result = await mediator.Send(new ResolveAddressByIdQuery(Guid.NewGuid(), Guid.NewGuid()));

        Assert.True(result.IsError);
        Assert.Equal("Address.NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_AddressBelongsToAnotherCustomer_ShouldReturnNotFound()
    {
        var (_, addressId) = await SeedAsync();
        var otherCustomerId = Guid.NewGuid();  
        var mediator = _factory.CreateMediator();

        var result = await mediator.Send(new ResolveAddressByIdQuery(addressId, otherCustomerId));

        Assert.True(result.IsError);
        Assert.Equal("Address.NotFound", result.TopError.Code);
    }
}