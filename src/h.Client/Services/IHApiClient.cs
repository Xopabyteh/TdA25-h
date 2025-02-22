using h.Contracts.Components.Services;
using h.Contracts.Users;
using Refit;

namespace h.Client.Services;

public interface IHApiClient : IWasmOnly
{
    [Post("/api/v1/users/login")]
    Task<ApiResponse<AuthenticationResponse>> LoginUser([Body] LoginUserRequest request);

    [Post("/api/v1/users/register")]
    Task<ApiResponse<AuthenticationResponse>> RegisterUser([Body] RegisterUserRequest request);
}
