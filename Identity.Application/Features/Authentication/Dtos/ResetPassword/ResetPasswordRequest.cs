namespace Identity.Application.Features.Authentication.Dtos.ResetPassword;

public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);