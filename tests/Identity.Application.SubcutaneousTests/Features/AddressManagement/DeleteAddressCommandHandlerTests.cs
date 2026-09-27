using Identity.Application.Features.AddressManagement.Commands.AddAddress;
using Identity.Application.Features.AddressManagement.Commands.DeleteAddress;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Location;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.AddressManagement;

[Collection(WebAppFactoryCollection.CollectionName)]
public class DeleteAddressCommandHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<(Guid customerId, Guid addressId)> SeedAddressAsync()
    {
        var context = _factory.CreateAppDbContext();
        var unique = Guid.NewGuid().ToString("N")[..8];
        var gov = Governorate.Create(Guid.NewGuid(), $"Gov_{unique}", $"GovE_{unique}").Value;
        var city = City.Create(Guid.NewGuid(), gov.Id, $"City_{unique}", $"CityE_{unique}").Value;
        context.Governorates.Add(gov);
        context.Cities.Add(city);
        await context.SaveChangesAsync(default);

        // ✅ Seed user حقيقي
        var customerId = await _factory.SeedConfirmedUserAsync();
        var mediator = _factory.CreateMediator(customerId);

        var added = await mediator.Send(new AddAddressCommand(
            new AddAddressRequest("Home", gov.Id, city.Id, "St", "1", null, null, null, null, null, false)));

        return (customerId, added.Value.Id);
    }

    [Fact]
    public async Task Handle_OwnAddress_ShouldDeleteSuccessfully()
    {
        var (customerId, addressId) = await SeedAddressAsync();
        var mediator = _factory.CreateMediator(customerId);

        var result = await mediator.Send(new DeleteAddressCommand(addressId, customerId));

        Assert.True(result.IsSuccess);

        var context = _factory.CreateAppDbContext();
        var stillExists = await context.Addresses.AnyAsync(a => a.Id == addressId);
        Assert.False(stillExists);
    }

    [Fact]
    public async Task Handle_WhenAddressDoesNotExist_ShouldFail()
    {
        var mediator = _factory.CreateMediator(Guid.NewGuid());
        var result = await mediator.Send(new DeleteAddressCommand(Guid.NewGuid(), Guid.NewGuid()));

        Assert.True(result.IsError);
        Assert.Equal("Address.NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_AnotherCustomerAddress_ShouldFail()
    {
        var (_, addressId) = await SeedAddressAsync();

        var otherCustomerId = await _factory.SeedConfirmedUserAsync();
        var mediator = _factory.CreateMediator(otherCustomerId);

        var result = await mediator.Send(new DeleteAddressCommand(addressId, otherCustomerId));

        Assert.True(result.IsError);
        Assert.Equal("Address.Unauthorized", result.TopError.Code);
    }
}