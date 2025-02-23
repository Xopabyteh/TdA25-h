using h.Contracts.Components.Services;
using h.Contracts.Users;

namespace h.Client.Services;

public interface IWasmCurrentUserStateService : IWasmOnly
{
    string Name { get; }
    bool IsGuest { get; set; }
    UserResponse? UserDetails { get; set; }
    Task EnsureStateAsync();
    void MarkShouldRefresh();
}
