using System.Net;
using System.Net.Http.Json;
using Identity.Api.IntegrationTests.Common;
using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Application.Features.Authentication.Dtos.Login;
using Identity.Domain.Identity;
using Xunit;

namespace Identity.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class AuthControllerTests(WebAppFactory webAppFactory)
{
    private readonly WebAppFactory _factory = webAppFactory;

    [Fact]
    public async Task RegisterConfirmLogin_FullFlow_ShouldSucceed()
    {
        var client = _factory.CreateAppHttpClient();
        var email = $"{Guid.NewGuid()}@localhost.test";

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Donia Tester"), "Name" },
            { new StringContent(email), "Email" },
            { new StringContent("01000000000"), "PhoneNumber" },
            { new StringContent("Str0ng!Pass"), "Password" },
            { new StringContent(Role.Customer.ToString()), "Role" }
        };

        var registerResponse = await client.SendFormAsync("api/auth/register", form);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<RegisterEnvelope>();
        var userId = registerBody!.data!.userId;

        var code = _factory.EmailService.LastConfirmationCode;
        Assert.NotNull(code);

        var confirmResponse = await client.PostAsJsonAsync(
            "api/auth/confirm-email",
            new ConfirmEmailRequest(userId.ToString(), code!));

        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync(
            "api/auth/login",
            new LoginRequest(email, "Str0ng!Pass"));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnconfirmedEmail_ShouldReturnForbidden()
    {
        var client = _factory.CreateAppHttpClient();
        var email = $"{Guid.NewGuid()}@localhost.test";

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Donia Tester"), "Name" },
            { new StringContent(email), "Email" },
            { new StringContent("01000000000"), "PhoneNumber" },
            { new StringContent("Str0ng!Pass"), "Password" },
            { new StringContent(Role.Customer.ToString()), "Role" }
        };

        await client.SendFormAsync("api/auth/register", form);

        var loginResponse = await client.PostAsJsonAsync(
            "api/auth/login",
            new LoginRequest(email, "Str0ng!Pass"));

        Assert.Equal(HttpStatusCode.Forbidden, loginResponse.StatusCode);
    }

    private sealed record RegisterEnvelope(RegisterData? data);

    private sealed record RegisterData(Guid userId, string email, string name);
}
