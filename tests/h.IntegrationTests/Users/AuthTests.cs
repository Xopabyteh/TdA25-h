using h.Contracts.Users;
using System.Net;
using System.Net.Http.Json;

namespace h.IntegrationTests.Auth;

public class AuthTests
{
    [ClassDataSource<CustomWebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public static CustomWebApplicationFactory _sessionApiFactory { get; set; } = null!;
    

    [Test]
    public async Task Register_ValidUser_ReturnsSuccess()
    {
        // Arrange
        using var client = _sessionApiFactory.CreateClient();
        var request = new RegisterUserRequest(
            "authTestUser1",
            "authTestEmail1@tda.h",
            "P@ssw0rd"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/register", request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Test]
    [Arguments("password")]
    [Arguments("P@ssw0r")]
    [Arguments("HESLO")]
    [Arguments("h3sl0")]
    public async Task Register_WeakPassword_ReturnsValidationErrors(string weakPassword)
    {
        // Arrange
        using var client = _sessionApiFactory.CreateClient();
        var request = new RegisterUserRequest(
            $"invaliduser1",
            $"invalidemail1@tda.h",
            weakPassword // Weak (invalid) password
        );
        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/register", request);
        
        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    [DependsOn(nameof(Register_ValidUser_ReturnsSuccess))]
    public async Task Login_ValidUser_ReturnsSuccess()
    {
        // Arrange
        using var client = _sessionApiFactory.CreateClient();
        var request = new LoginUserRequest(
            "authTestUser1",
            "P@ssw0rd"
        );
        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/login", request);
        
        // Assert
        response.EnsureSuccessStatusCode();
    }
}
