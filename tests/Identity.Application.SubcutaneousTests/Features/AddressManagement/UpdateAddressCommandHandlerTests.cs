using Identity.Application.Features.AddressManagement.Commands.AddAddress;
using Identity.Application.Features.AddressManagement.Commands.UpdateAddress;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Application.Features.AddressManagement.Dtos.UpdateAddress;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Location;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.AddressManagement;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateAddressCommandHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<(Guid customerId, Guid addressId, Governorate gov, City city)> SeedAsync()
    {
        var context = _factory.CreateAppDbContext();
        var unique = Guid.NewGuid().ToString("N")[..8];
        var gov = Governorate.Create(Guid.NewGuid(), $"GU_{unique}", $"GUE_{unique}").Value;
        var city = City.Create(Guid.NewGuid(), gov.Id, $"CU_{unique}", $"CUE_{unique}").Value;
        context.Governorates.Add(gov);
        context.Cities.Add(city);
        await context.SaveChangesAsync(default);

        var customerId = await _factory.SeedConfirmedUserAsync();  
        var mediator = _factory.CreateMediator(customerId);

        var added = await mediator.Send(new AddAddressCommand(
            new AddAddressRequest("Home", gov.Id, city.Id, "St", "1", null, null, null, null, null, false)));

        return (customerId, added.Value.Id, gov, city);
    }

    [Fact]
    public async Task Handle_OwnAddress_ShouldUpdateSuccessfully()
    {
        var (customerId, addressId, gov, city) = await SeedAsync();
        var mediator = _factory.CreateMediator(customerId);

        var request = new UpdateAddressRequest("New Label", gov.Id, city.Id, "New St", "99", null, null, null, null, null);
        var result = await mediator.Send(new UpdateAddressCommand(addressId, customerId, request));

        Assert.True(result.IsSuccess);
        Assert.Equal("New Label", result.Value.Label);
    }

    [Fact]
    public async Task Handle_AddressDoesNotExist_ShouldFail()
    {
        var mediator = _factory.CreateMediator(Guid.NewGuid());
        var request = new UpdateAddressRequest("X", Guid.NewGuid(), Guid.NewGuid(), "St", "1", null, null, null, null, null);

        var result = await mediator.Send(new UpdateAddressCommand(Guid.NewGuid(), Guid.NewGuid(), request));

        Assert.True(result.IsError);
        Assert.Equal("Address.NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_AnotherCustomerAddress_ShouldFail()
    {
        var (_, addressId, gov, city) = await SeedAsync();
        var otherId = await _factory.SeedConfirmedUserAsync();  
        var mediator = _factory.CreateMediator(otherId);

        var request = new UpdateAddressRequest("X", gov.Id, city.Id, "St", "1", null, null, null, null, null);
        var result = await mediator.Send(new UpdateAddressCommand(addressId, otherId, request));

        Assert.True(result.IsError);
        Assert.Equal("Address.Unauthorized", result.TopError.Code);
    }
}