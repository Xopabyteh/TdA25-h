using h.Contracts.Users;
using System.Net;
using System.Net.Http.Json;

namespace h.IntegrationTests.Auth;

public class AuthTests
{
    [ClassDataSource<CustomWebApplicationFactory>(Shared = SharedType.PerClass)]
    public static CustomWebApplicationFactory _classApiFactory { get; set; } = null!;
    

    [Test]
    public async Task Register_ValidUser_ReturnsSuccess()
    {
        // Arrange
        using var client = _classApiFactory.CreateClient();
        var request = new RegisterUserRequest(
            "user1",
            "email1@tda.h",
            "P@ssw0rd"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/register", request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task Register_InvalidUser_ReturnsBadRequest()
    {
        // Arrange
        using var client = _classApiFactory.CreateClient();
        var request = new RegisterUserRequest(
            "invaliduser1",
            "invalidemail1@tda.h",
            "password" // Weak (invalid) password
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
        using var client = _classApiFactory.CreateClient();
        var request = new LoginUserRequest(
            "user1",
            "P@ssw0rd"
        );
        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/login", request);
        
        // Assert
        response.EnsureSuccessStatusCode();
    }
}
