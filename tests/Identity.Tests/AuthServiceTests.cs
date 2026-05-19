using Identity.Application.Abstractions;
using Identity.Application.Auth;
using Identity.Domain.Entities;
using Moq;

namespace Identity.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesUserAndReturnsToken()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByEmailAsync("user@mail.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);

        var jwt = new Mock<IJwtTokenGenerator>();
        jwt.Setup(x => x.Generate(It.IsAny<AppUser>())).Returns("token");

        var service = new AuthService(users.Object, jwt.Object);

        var result = await service.RegisterAsync(new RegisterRequest("user@mail.com", "User Name", "Password1", "User"));

        Assert.Equal("token", result.Token);
        Assert.Equal("user@mail.com", result.Email);
        users.Verify(x => x.AddAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Once);
        users.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenEmailExists()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppUser("user@mail.com", "User Name", "hash", "User"));

        var service = new AuthService(users.Object, Mock.Of<IJwtTokenGenerator>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(new RegisterRequest("user@mail.com", "User Name", "Password1", "User")));
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenPasswordIsWrong()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Password1");
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppUser("user@mail.com", "User Name", hash, "User"));

        var service = new AuthService(users.Object, Mock.Of<IJwtTokenGenerator>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("user@mail.com", "wrong")));
    }
}
