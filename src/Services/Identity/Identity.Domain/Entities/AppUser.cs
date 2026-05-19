namespace Identity.Domain.Entities;

public sealed class AppUser
{
    private AppUser() { }

    public AppUser(string email, string fullName, string passwordHash, string role)
    {
        Id = Guid.NewGuid();
        Email = NormalizeEmail(email);
        FullName = string.IsNullOrWhiteSpace(fullName) ? throw new ArgumentException("Full name is required.") : fullName.Trim();
        PasswordHash = string.IsNullOrWhiteSpace(passwordHash) ? throw new ArgumentException("Password hash is required.") : passwordHash;
        Role = role is "Admin" or "User" ? role : throw new ArgumentException("Role must be Admin or User.");
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = "User";
    public DateTime CreatedAtUtc { get; private set; }

    public void ChangeRole(string role)
    {
        Role = role is "Admin" or "User" ? role : throw new ArgumentException("Role must be Admin or User.");
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Valid email is required.");

        return email.Trim().ToLowerInvariant();
    }
}
