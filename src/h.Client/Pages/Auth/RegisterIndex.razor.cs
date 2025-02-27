using h.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Runtime.InteropServices;
using FluentValidation;
using h.Contracts.Users;
using h.Contracts;

namespace h.Client.Pages.Auth;

public partial class RegisterIndex
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

    private async Task HandleRegister()
    {
        var request = new RegisterUserRequest(Model.Username, Model.Email, Model.Password);

        var response = await _api.RegisterUser(request);
       

        if (response.IsSuccessStatusCode)
        {
            if(_authProvider is WasmAuthenticationStateProvider _wasmAuth)
            {
                _wasmAuth.MarkShouldReloadAuthState();
            }
            _navigation.NavigateTo(ReturnUrl ?? "/", forceLoad: true);
            return;
        }

        // -> Error
        var error = response.Error.ToErrorResponse();
        if (error.TryFindError(nameof(SharedErrors.User.UsernameAlreadyTaken), out _))
        {
            await _toast.ErrorAsync("Uživatelské jméno je již zabrané");
        }
        else if (error.TryFindError(nameof(SharedErrors.User.EmailAlreadyTaken), out _))
        {
            await _toast.ErrorAsync("Email je již zabraný");
        }
        else
        {
            await _toast.ErrorAsync(error.Message);
        }
    }

    public class RequestModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordMatch { get; set; }
    }

    public class RequestValidator : AbstractValidator<RequestModel>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Username).SetValidator(new SharedUsernameValidator());
            RuleFor(x => x.Email).NotNull();
            RuleFor(x => x.Password).SetValidator(new SharedPasswordValidator());
            RuleFor(x => x.PasswordMatch).Equal(x => x.Password).WithMessage("Hesla se neshodují");
        }
    }
}