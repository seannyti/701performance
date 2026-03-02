using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PowersportsApi.Data;
using PowersportsApi.Models;
using PowersportsApi.Models.Auth;
using PowersportsApi.Services;

namespace PowersportsApi.Tests.Services;

/// <summary>
/// Unit tests for AuthService covering registration, login, token refresh, and logout flows.
/// Uses an in-memory database to avoid SQL Server dependency during test runs.
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly PowersportsDbContext _context;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<PowersportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PowersportsDbContext(options);
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        SetupJwtConfiguration();

        _authService = new AuthService(_context, _configMock.Object, _loggerMock.Object);
    }

    private void SetupJwtConfiguration()
    {
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(s => s["SecretKey"]).Returns("test-jwt-secret-key-that-is-at-least-32-characters-long!");
        jwtSection.Setup(s => s["Issuer"]).Returns("TestIssuer");
        jwtSection.Setup(s => s["Audience"]).Returns("TestAudience");

        _configMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSection.Object);
        _configMock.Setup(c => c["JwtSettings:AccessTokenExpiryMinutes"]).Returns("60");
        _configMock.Setup(c => c["JwtSettings:RefreshTokenExpiryDays"]).Returns("7");
    }

    // ─── Registration ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsSuccessWithToken()
    {
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecurePass1"
        };

        var result = await _authService.RegisterAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.User.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ReturnsFailure()
    {
        var email = "duplicate@example.com";
        await CreateTestUser(email);

        var request = new RegisterRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = email,
            Password = "SecurePass1"
        };

        var result = await _authService.RegisterAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task RegisterAsync_EmailStoredAsLowerCase()
    {
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "UPPERCASE@EXAMPLE.COM",
            Password = "SecurePass1"
        };

        var result = await _authService.RegisterAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.User.Email.Should().Be("uppercase@example.com");
    }

    [Fact]
    public async Task RegisterAsync_NewUserHasDefaultUserRole()
    {
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "newuser@example.com",
            Password = "SecurePass1"
        };

        var result = await _authService.RegisterAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.User.Role.Should().Be(UserRole.User);
    }

    // ─── Login ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessWithToken()
    {
        var email = "login.test@example.com";
        var password = "SecurePass1";
        await CreateTestUser(email, password);

        var result = await _authService.LoginAsync(new LoginRequest { Email = email, Password = password });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
    {
        await CreateTestUser("wrongpass@example.com", "CorrectPass1");

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "wrongpass@example.com",
            Password = "WrongPassword1"
        });

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ReturnsFailure()
    {
        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "SomePass1"
        });

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsFailure()
    {
        await CreateTestUser("inactive@example.com", "SecurePass1", isActive: false);

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "inactive@example.com",
            Password = "SecurePass1"
        });

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_DoesNotRevealWhetherEmailExists()
    {
        var existingEmail = "exists@example.com";
        await CreateTestUser(existingEmail, "CorrectPass1");

        var wrongEmailResult = await _authService.LoginAsync(new LoginRequest
        {
            Email = "notexists@example.com",
            Password = "WrongPass1"
        });

        var wrongPasswordResult = await _authService.LoginAsync(new LoginRequest
        {
            Email = existingEmail,
            Password = "WrongPass1"
        });

        // Both should return the same error message so attackers can't enumerate emails
        wrongEmailResult.ErrorMessage.Should().Be(wrongPasswordResult.ErrorMessage);
    }

    // ─── Token Refresh ────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
    {
        await CreateTestUser("refresh@example.com", "SecurePass1");
        var loginResult = await _authService.LoginAsync(new LoginRequest
        {
            Email = "refresh@example.com",
            Password = "SecurePass1"
        });
        var refreshToken = loginResult.Data!.RefreshToken;

        var result = await _authService.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBe(refreshToken, "a new refresh token should be issued each time");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ReturnsFailure()
    {
        var result = await _authService.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = "completely-invalid-token"
        });

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ReturnsFailure()
    {
        var user = await CreateTestUser("expired@example.com");

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = "expired-token-xyz",
            ExpiryDate = DateTime.UtcNow.AddDays(-1), // already expired
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        });
        await _context.SaveChangesAsync();

        var result = await _authService.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = "expired-token-xyz"
        });

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid refresh token");
    }

    // ─── Logout ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_InvalidatesRefreshToken()
    {
        await CreateTestUser("logout@example.com", "SecurePass1");
        var loginResult = await _authService.LoginAsync(new LoginRequest
        {
            Email = "logout@example.com",
            Password = "SecurePass1"
        });
        var refreshToken = loginResult.Data!.RefreshToken;

        await _authService.LogoutAsync(new LogoutRequest { RefreshToken = refreshToken });

        var refreshResult = await _authService.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken });
        refreshResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutAsync_WithUnknownToken_StillSucceeds()
    {
        var result = await _authService.LogoutAsync(new LogoutRequest { RefreshToken = "non-existent-token" });

        result.IsSuccess.Should().BeTrue();
    }

    // ─── GetUserById ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        var user = await CreateTestUser("getuser@example.com");

        var result = await _authService.GetUserByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Email.Should().Be("getuser@example.com");
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
    {
        var result = await _authService.GetUserByIdAsync(99999);

        result.Should().BeNull();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<User> CreateTestUser(string email, string password = "TestPass1!", bool isActive = true)
    {
        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.User,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
