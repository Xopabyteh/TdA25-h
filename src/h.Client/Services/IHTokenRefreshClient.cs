using h.Contracts.Components.Services;
using Refit;

namespace h.Client.Services;

public interface IHTokenRefreshClient : IWasmOnly
{
    [Post("/api/v1/users/refresh-token")]
    Task<ApiResponse<string>> RefreshToken();
}
