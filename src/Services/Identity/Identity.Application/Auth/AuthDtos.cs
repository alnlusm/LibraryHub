namespace Identity.Application.Auth;

public sealed record RegisterRequest(string Email, string FullName, string Password, string Role = "User");
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(Guid UserId, string Email, string FullName, string Role, string Token);
public sealed record UserProfileResponse(Guid UserId, string Email, string FullName, string Role);
