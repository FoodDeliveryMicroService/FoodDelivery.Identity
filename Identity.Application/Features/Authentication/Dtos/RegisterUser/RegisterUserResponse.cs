namespace Identity.Application.Features.Authentication.Dtos.RegisterUser
{
    public sealed record RegisterUserResponse(Guid UserId, string Email, string Name);

}
