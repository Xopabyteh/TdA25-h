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
            .MinimumLength(3)
            .MaximumLength(16);
    }
}
