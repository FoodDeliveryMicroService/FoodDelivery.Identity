using System.Net.Http.Headers;
using System.Net.Http.Json;
using Identity.Application.Features.Authentication.Dtos.Login;

namespace Identity.Api.IntegrationTests.Common;

public class AppHttpClient(HttpClient httpClient)
{
    public async Task<string> LoginAsync(string email, string password)
    {
        var response = await httpClient.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password));

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Login failed with status code {response.StatusCode}");
        }

        var envelope = await response.Content.ReadFromJsonAsync<LoginEnvelope>();

        return envelope?.data?.accessToken
            ?? throw new InvalidOperationException("Login response did not contain an access token.");
    }

    public void SetAuthorizationHeader(string token) =>
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public Task<HttpResponseMessage> GetAsync(string requestUri) =>
        httpClient.GetAsync(requestUri);

    public Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value) =>
        httpClient.PostAsJsonAsync(requestUri, value);

    public Task<HttpResponseMessage> SendFormAsync(string requestUri, MultipartFormDataContent form) =>
        httpClient.PostAsync(requestUri, form);

    public Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value) =>
        httpClient.PutAsJsonAsync(requestUri, value);

    public Task<HttpResponseMessage> SendPutFormAsync(string requestUri, MultipartFormDataContent form) =>
        httpClient.PutAsync(requestUri, form);

    public Task<HttpResponseMessage> DeleteAsync(string requestUri) =>
        httpClient.DeleteAsync(requestUri);

    // Mirrors ApiController's envelope: { statusCode, message, errors, data }
    private sealed record LoginEnvelope(LoginResponseData? data);

    private sealed record LoginResponseData(string accessToken, string refreshToken, DateTimeOffset expiresAt);
}
