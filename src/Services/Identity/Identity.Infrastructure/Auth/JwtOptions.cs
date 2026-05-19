namespace Identity.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "LibraryHub";
    public string Audience { get; init; } = "LibraryHub";
    public string Secret { get; init; } = "CHANGE_ME_TO_A_LONG_SECRET_32_CHARS";
    public int ExpirationMinutes { get; init; } = 120;
}
