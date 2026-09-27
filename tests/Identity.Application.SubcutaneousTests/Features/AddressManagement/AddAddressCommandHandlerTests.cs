using Identity.Application.Features.AddressManagement.Commands.AddAddress;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Application.SubcutaneousTests.Common;
using Identity.Domain.Location;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Identity.Application.SubcutaneousTests.Features.AddressManagement;

[Collection(WebAppFactoryCollection.CollectionName)]
public class AddAddressCommandHandlerTests(WebAppFactory factory)
{
    private readonly WebAppFactory _factory = factory;

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateAddress()
    {
        var (gov, city) = await SetupLocationAsync();

        var customerId = await _factory.SeedConfirmedUserAsync();
        var mediator = _factory.CreateMediator(customerId);

        var request = new AddAddressRequest(
            "Home", gov.Id, city.Id, "St", "1",
            null, null, null, null, null, false);

        var result = await mediator.Send(new AddAddressCommand(request));

        Assert.True(result.IsSuccess);
        Assert.Equal("Home", result.Value.Label);
    }

    [Fact]
    public async Task Handle_WithDefaultAddress_ShouldUnsetPreviousDefault()
    {
        var (gov, city) = await SetupLocationAsync();
        var customerId = await _factory.SeedConfirmedUserAsync();  
        var mediator = _factory.CreateMediator(customerId);

        await mediator.Send(new AddAddressCommand(new AddAddressRequest(
            "Home", gov.Id, city.Id, "St", "1", null, null, null, null, null, true)));

        await mediator.Send(new AddAddressCommand(new AddAddressRequest(
            "Work", gov.Id, city.Id, "St2", "2", null, null, null, null, null, true)));

        var context = _factory.CreateAppDbContext();
        var addresses = await context.Addresses
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();

        Assert.Equal(2, addresses.Count);
        Assert.Single(addresses.Where(a => a.IsDefault));
        Assert.Equal("Work", addresses.Single(a => a.IsDefault).Label);
    }

    [Fact]
    public async Task Handle_WithNonDefaultAddress_ShouldKeepExistingDefault()
    {
        var (gov, city) = await SetupLocationAsync();
        var customerId = await _factory.SeedConfirmedUserAsync();  
        var mediator = _factory.CreateMediator(customerId);

        await mediator.Send(new AddAddressCommand(new AddAddressRequest(
            "Home", gov.Id, city.Id, "St", "1", null, null, null, null, null, true)));

        await mediator.Send(new AddAddressCommand(new AddAddressRequest(
            "Work", gov.Id, city.Id, "St2", "2", null, null, null, null, null, false)));

        var context = _factory.CreateAppDbContext();
        var defaultAddress = await context.Addresses
            .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.IsDefault);

        Assert.NotNull(defaultAddress);
        Assert.Equal("Home", defaultAddress!.Label);
    }
    private async Task<(Governorate gov, City city)> SetupLocationAsync()
    {
        var context = _factory.CreateAppDbContext();
        var unique = Guid.NewGuid().ToString("N")[..8];
        var gov = Governorate.Create(Guid.NewGuid(), $"محافظة_{unique}", $"Gov_{unique}").Value;
        var city = City.Create(Guid.NewGuid(), gov.Id, $"مدينة_{unique}", $"City_{unique}").Value;
        context.Governorates.Add(gov);
        context.Cities.Add(city);
        await context.SaveChangesAsync(default);
        return (gov, city);
    }

    

    [Fact]
    public async Task Handle_WhenCurrentUserIsMissing_ShouldFail()
    {
        var (gov, city) = await SetupLocationAsync();
        var mediator = _factory.CreateMediator(null);

        var request = new AddAddressRequest("Home", gov.Id, city.Id, "St", "1", null, null, null, null, null, false);
        var result = await mediator.Send(new AddAddressCommand(request));

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_WithInvalidGovernorate_ShouldFail()
    {
        var (_, city) = await SetupLocationAsync();
        var mediator = _factory.CreateMediator(Guid.NewGuid());

        var request = new AddAddressRequest("Home", Guid.NewGuid(), city.Id, "St", "1", null, null, null, null, null, false);
        var result = await mediator.Send(new AddAddressCommand(request));

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_WithInvalidCity_ShouldFail()
    {
        var (gov, _) = await SetupLocationAsync();
        var mediator = _factory.CreateMediator(Guid.NewGuid());

        var request = new AddAddressRequest("Home", gov.Id, Guid.NewGuid(), "St", "1", null, null, null, null, null, false);
        var result = await mediator.Send(new AddAddressCommand(request));

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_WhenCityDoesNotBelongToGovernorate_ShouldFail()
    {
        var context = _factory.CreateAppDbContext();
        var unique = Guid.NewGuid().ToString("N")[..8];
        var gov1 = Governorate.Create(Guid.NewGuid(), $"G1_{unique}", $"Gov1_{unique}").Value;
        var gov2 = Governorate.Create(Guid.NewGuid(), $"G2_{unique}", $"Gov2_{unique}").Value;
        var city = City.Create(Guid.NewGuid(), gov1.Id, $"C_{unique}", $"City_{unique}").Value;
        context.Governorates.AddRange(gov1, gov2);
        context.Cities.Add(city);
        await context.SaveChangesAsync(default);

        var mediator = _factory.CreateMediator(Guid.NewGuid());
        var request = new AddAddressRequest("Home", gov2.Id, city.Id, "St", "1", null, null, null, null, null, false);
        var result = await mediator.Send(new AddAddressCommand(request));

        Assert.True(result.IsError);
        Assert.Equal("Address.Location.Invalid", result.TopError.Code);
    }

}