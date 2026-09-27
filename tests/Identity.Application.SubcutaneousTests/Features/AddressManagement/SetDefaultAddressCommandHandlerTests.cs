using Identity.Application.Features.AddressManagement.Commands.AddAddress;
using Identity.Application.Features.AddressManagement.Commands.SetDefaultAddress;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Location;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.AddressManagement;

[Collection(WebAppFactoryCollection.CollectionName)]
public class SetDefaultAddressCommandHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    private async Task<(Guid customerId, Governorate gov, City city)> SetupAsync()
    {
        var context = _factory.CreateAppDbContext();
        var unique = Guid.NewGuid().ToString("N")[..8];
        var gov = Governorate.Create(Guid.NewGuid(), $"GS_{unique}", $"GSE_{unique}").Value;
        var city = City.Create(Guid.NewGuid(), gov.Id, $"CS_{unique}", $"CSE_{unique}").Value;
        context.Governorates.Add(gov);
        context.Cities.Add(city);
        await context.SaveChangesAsync(default);

        var customerId = await _factory.SeedConfirmedUserAsync();   // ✅
        return (customerId, gov, city);
    }

    private static AddAddressRequest MakeRequest(string label, Governorate g, City c, bool isDefault = false) =>
        new(label, g.Id, c.Id, "St", "1", null, null, null, null, null, isDefault);

    [Fact]
    public async Task Handle_OwnAddress_ShouldBecomeDefault()
    {
        var (customerId, gov, city) = await SetupAsync();
        var mediator = _factory.CreateMediator(customerId);

        var added = await mediator.Send(new AddAddressCommand(MakeRequest("Home", gov, city)));
        var result = await mediator.Send(new SetDefaultAddressCommand(added.Value.Id, customerId));

        Assert.True(result.IsSuccess);

        var context = _factory.CreateAppDbContext();
        var address = await context.Addresses.FindAsync(added.Value.Id);
        Assert.True(address!.IsDefault);
    }

    [Fact]
    public async Task Handle_ShouldUnsetPreviousDefault()
    {
        var (customerId, gov, city) = await SetupAsync();
        var mediator = _factory.CreateMediator(customerId);

        var first = await mediator.Send(new AddAddressCommand(MakeRequest("Home", gov, city, true)));
        var second = await mediator.Send(new AddAddressCommand(MakeRequest("Work", gov, city)));

        await mediator.Send(new SetDefaultAddressCommand(second.Value.Id, customerId));

        var context = _factory.CreateAppDbContext();
        var addresses = await context.Addresses.Where(a => a.CustomerId == customerId).ToListAsync();

        Assert.Single(addresses.Where(a => a.IsDefault));
        Assert.Equal(second.Value.Id, addresses.Single(a => a.IsDefault).Id);
    }

    [Fact]
    public async Task Handle_AddressDoesNotExist_ShouldFail()
    {
        var mediator = _factory.CreateMediator(Guid.NewGuid());
        var result = await mediator.Send(new SetDefaultAddressCommand(Guid.NewGuid(), Guid.NewGuid()));

        Assert.True(result.IsError);
        Assert.Equal("Address.NotFound", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_AnotherCustomerAddress_ShouldFail()
    {
        var (customerId, gov, city) = await SetupAsync();
        var mediator = _factory.CreateMediator(customerId);
        var added = await mediator.Send(new AddAddressCommand(MakeRequest("Home", gov, city)));

        var otherCustomerId = await _factory.SeedConfirmedUserAsync();   // ✅
        var otherMediator = _factory.CreateMediator(otherCustomerId);
        var result = await otherMediator.Send(new SetDefaultAddressCommand(added.Value.Id, otherCustomerId));

        Assert.True(result.IsError);
        Assert.Equal("Address.Unauthorized", result.TopError.Code);
    }
}