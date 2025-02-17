using FluentValidation;

namespace h.Contracts.Users;

/// <summary>
/// HESLO: 
/// min. 8 znaků
/// musí obsahovat 1 speciální znak
/// musí obsahovat 1 číslovku
/// musí obsahovat 1 malé písmeno
/// musí obsahovat 1 VELKÉ písmeno
/// </summary>
public sealed class SharedPasswordValidator : AbstractValidator<string>
{
    public SharedPasswordValidator()
    {
        RuleFor(password => password)
            .NotEmpty().WithMessage("Heslo nesmí být prázdné.")
            .MinimumLength(8).WithMessage("Heslo musí mít alespoň 8 znaků.")
            .Matches(@"[A-Z]").WithMessage("Heslo musí obsahovat alespoň jedno velké písmeno.")
            .Matches(@"[a-z]").WithMessage("Heslo musí obsahovat alespoň jedno malé písmeno.")
            .Matches(@"\d").WithMessage("Heslo musí obsahovat alespoň jednu číslici.")
            .Matches("[!@#$%^&*(),.?\":{}|<>]").WithMessage("Heslo musí obsahovat alespoň jeden speciální znak.");
    }
}
