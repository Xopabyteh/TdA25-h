using FluentValidation;
using h.Client.Services;
using h.Contracts;
using h.Contracts.Users;

namespace h.Client.Pages.Admin;

public partial class AdminPanel
{
    private readonly IHApiClient _api;
    private readonly ToastService _toast;

    public AdminPanel(IHApiClient api, ToastService toast)
    {
        _api = api;
        _toast = toast;
    }

    private string query = string.Empty;
    private UserResponse? _user;
    private bool isChangingPassword;
    protected RequestModel Model { get; set; } = new();

    private async Task FindUser()
    {
        isChangingPassword = false;
        var result = await _api.FindUser(query);

        if(result.IsSuccessStatusCode)
        {
            _user = result.Content;
            UpdateModelFromUser();
            return;
        }

        await _toast.ErrorAsync("Uživatel nenalezen");
    }

    private async Task HandleUpdate()
    {
        var request = new AdminUpdateUserRequest(
            Model.Username,
            Model.Email,
            Model.Password,
            Model.Rating,
            Model.WinAmount,
            Model.DrawAmount,
            Model.LossAmount
        );

        var response = await _api.AdminUpdateUser(_user!.Value.Uuid, request);
        if (response.IsSuccessStatusCode)
        {
            await _toast.SuccessAsync("Uloženo");
            _user = response.Content;
            UpdateModelFromUser();
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

    private void UpdateModelFromUser()
    {
        if (_user is null)
            return;

        Model = new RequestModel
        {
            Username = _user.Value.Username,
            Email = _user.Value.Email,
            Rating = _user.Value.Elo,
            WinAmount = _user.Value.Wins,
            DrawAmount = _user.Value.Draws,
            LossAmount = _user.Value.Losses
        };
    }

    public class RequestModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public int Rating { get; set; }
        public int WinAmount { get; set; }
        public int DrawAmount { get; set; }
        public int LossAmount { get; set; }
    }

    public class RequestValidator : AbstractValidator<RequestModel>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Username).SetValidator(new SharedUsernameValidator());
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).SetValidator(new SharedPasswordValidator());

            RuleFor(x => x.Rating).GreaterThanOrEqualTo(0);
            RuleFor(x => x.WinAmount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DrawAmount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.LossAmount).GreaterThanOrEqualTo(0);
        }
    }
}