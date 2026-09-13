namespace Identity.Application.Features.Authentication.Dtos.Email
{
    public sealed record ConfirmEmailRequest(string userId, string Code);
}
