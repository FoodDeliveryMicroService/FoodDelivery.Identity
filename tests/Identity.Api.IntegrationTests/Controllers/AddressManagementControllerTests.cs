using System.Net;
using Identity.Api.IntegrationTests.Common;
using Identity.Application.Common.Interfaces;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Identity.Domain.Identity;
using Identity.Domain.Location;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Identity.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class AddressManagementControllerTests(WebAppFactory webAppFactory)
{
    private readonly WebAppFactory _factory = webAppFactory;
    private readonly IAppDbContext _context = webAppFactory.CreateAppDbContext();

    private async Task<AppHttpClient> CreateAuthenticatedCustomerClientAsync()
    {
        var email = $"{Guid.NewGuid()}@localhost.test";
        const string password = "Str0ng!Pass";

        await _factory.SeedConfirmedUserAsync(email, password, "Test Customer", "01000000000", Role.Customer);

        var client = _factory.CreateAppHttpClient();
        var token = await client.LoginAsync(email, password);
        client.SetAuthorizationHeader(token);

        return client;
    }
    [Fact]
    public async Task AddAddress_WithValidRequest_ShouldReturnCreated()
    {
        var client = await CreateAuthenticatedCustomerClientAsync();

        // ✅ استخدمي المحافظة/المدينة من الـ Seeder
        var governorate = await _context.Governorates.FirstAsync();
        var city = await _context.Cities
            .FirstAsync(c => c.GovernorateId == governorate.Id);

        var request = new AddAddressRequest(
            "Home", governorate.Id, city.Id,
            "Some St.", "1", null, null, null, null, null, IsDefault: true);

        var response = await client.PostAsJsonAsync("api/addressmanagement", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AddAddress_WithMismatchedCityAndGovernorate_ShouldReturnBadRequest()
    {
        var client = await CreateAuthenticatedCustomerClientAsync();

        // ✅ هات محافظتين مختلفتين من البيانات المزروعة
        var governorate1 = await _context.Governorates.FirstAsync();
        var governorate2 = await _context.Governorates
            .Where(g => g.Id != governorate1.Id)
            .FirstAsync();

        // ✅ هات مدينة تابعة للمحافظة الأولى
        var city = await _context.Cities
            .FirstAsync(c => c.GovernorateId == governorate1.Id);

        // نرسل governorate2 (اللي مش مالكة للمدينة) → المفروض يفشل
        var request = new AddAddressRequest(
            "Home", governorate2.Id, city.Id,
            "St", "1", null, null, null, null, null, false);

        var response = await client.PostAsJsonAsync("api/addressmanagement", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    [Fact]
    public async Task GetMyAddresses_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateAppHttpClient();

        var response = await client.GetAsync("api/addressmanagement");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    } 
}
