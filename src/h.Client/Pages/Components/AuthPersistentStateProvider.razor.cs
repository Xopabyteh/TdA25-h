using h.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace h.Client.Pages.Components;

/// <summary>
/// Stores auth state so clie
/// </summary>
public class AuthPersistentStateProvider : ComponentBase, IDisposable
{
    public const string ClaimsKey = "p-persister-auth-state-claims";

    private readonly PersistentComponentState _persistentComponentState;
    private PersistingComponentStateSubscription persistingSubscription;
    private readonly AuthenticationStateProvider _authStateProvider;

    private AuthenticationState? authResult;
    public AuthPersistentStateProvider(
        PersistentComponentState persistentComponentState,
        AuthenticationStateProvider authStateProvider)
    {
        _persistentComponentState = persistentComponentState;
        _authStateProvider = authStateProvider;
    }


    protected override async Task OnInitializedAsync()
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Wasm)
        {
            // Wasm
            var isPresent = _persistentComponentState.TryTakeFromJson(ClaimsKey, out ClaimView[]? claimViews);
            if(!isPresent)
                return;

            if(_authStateProvider is not WasmAuthenticationStateProvider wasmAuth)
                return;

            var claims = claimViews!.Select(c => new Claim(c.Type, c.Value)).ToArray();
            wasmAuth.MarkUserAsAuthenticated(claims); 
        }
        else
        {
            // Prerendering -> Persist
            authResult = await _authStateProvider.GetAuthenticationStateAsync();
            persistingSubscription = _persistentComponentState.RegisterOnPersisting(Persist);
        }
    }

    private Task Persist()
    {
        var claims = authResult?.User.Claims.Select(x => new ClaimView(x.Type, x.Value)).ToArray();
        _persistentComponentState.PersistAsJson(ClaimsKey, claims);
    
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        persistingSubscription.Dispose();
    }

    public readonly record struct ClaimView(string Type, string Value);
}
