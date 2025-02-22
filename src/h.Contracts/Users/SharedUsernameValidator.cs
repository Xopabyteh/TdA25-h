using FluentValidation;

namespace h.Contracts.Users;
/// <summary>
/// Max 16 characters
/// </summary>
public class SharedUsernameValidator : AbstractValidator<string>
{
    public SharedUsernameValidator()
    {
        RuleFor(username => username)
            .NotEmpty()
            .MinimumLength(3).WithMessage("Jméno musí mít alespoň 3 znaky")
            .MaximumLength(16).WithMessage("Jméno může mít maximálně 16 znaků");
    }
}
