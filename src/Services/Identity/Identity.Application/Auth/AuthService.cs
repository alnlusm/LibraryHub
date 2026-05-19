using Identity.Application.Abstractions;
using Identity.Domain.Entities;

namespace Identity.Application.Auth;

public sealed class AuthService
{
    private readonly IUserRepository _users;
    private readonly IJwtTokenGenerator _jwt;

    public AuthService(IUserRepository users, IJwtTokenGenerator jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("User with this email already exists.");

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new AppUser(request.Email, request.FullName, hash, request.Role);

        await _users.AddAsync(user, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);

        return ToAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return ToAuthResponse(user);
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        return new UserProfileResponse(user.Id, user.Email, user.FullName, user.Role);
    }

    private AuthResponse ToAuthResponse(AppUser user) =>
        new(user.Id, user.Email, user.FullName, user.Role, _jwt.Generate(user));
}
