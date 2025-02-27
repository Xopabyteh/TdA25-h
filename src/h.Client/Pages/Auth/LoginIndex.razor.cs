using h.Contracts.Users;
using Microsoft.AspNetCore.Components;
using h.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Runtime.InteropServices;

namespace h.Client.Pages.Auth;

public partial class LoginIndex
{
    [SupplyParameterFromQuery(Name = "return")] string? ReturnUrl { get; set; }
    [Inject] protected IHApiClient _api { get; set; } = null!;
    [Inject] protected ToastService _toast { get; set; } = null!;
    [Inject] protected AuthenticationStateProvider _authProvider { get; set; } = null!;
    [Inject] protected NavigationManager _navigation { get; set; } = null!;

    private bool isLoaded;
    private bool isBusy;
    private RequestModel Model { get; set; }

    protected override void OnInitialized()
    {
        // Load after mode is wasm
        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return;

        Model = new();
        isLoaded = true;
    }

    private async Task HandleLogin()
    {
        isBusy = true;
        var request = new LoginUserRequest(Model.Nickname, Model.Password);
        var response = await _api.LoginUser(request);
        isBusy = false;
        if(response.IsSuccessStatusCode)
        {
            if(_authProvider is WasmAuthenticationStateProvider _wasmAuth)
            {
                _wasmAuth.MarkShouldReloadAuthState();
            }
            _navigation.NavigateTo(ReturnUrl ?? PageRoutes.HomeIndex, forceLoad: true);
            return;
        }
        
        if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _toast.ErrorAsync("Špatné údaje");
            return;
        }
    }

    private class RequestModel
    {
        public string Nickname { get; set; }
        public string Password { get; set; }
    }
}