using Ardalis.SmartEnum;
using FluentValidation;
using FluentValidation.Validators;

namespace h.Server.Infrastructure;

public static class IsInSmartEnumFluentValidationExtension
{
    public class SmartEnumValidator<T, TProperty> : PropertyValidator<T, TProperty>
        where TProperty : SmartEnum<TProperty>
    {
        public override string Name => "SmartEnumValidator";

        private readonly IReadOnlyCollection<TProperty> avaliableEnumValues;

        public SmartEnumValidator(IReadOnlyCollection<TProperty> avaliableEnumValues)
        {
            this.avaliableEnumValues = avaliableEnumValues;
        }

        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            return avaliableEnumValues.Contains(value);
        }
    }

    public static IRuleBuilderOptions<T, TProperty> IsInSmartEnum<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        IReadOnlyCollection<TProperty> availableEnumValues)
        where TProperty : SmartEnum<TProperty>
    {
        return ruleBuilder.SetValidator(new SmartEnumValidator<T, TProperty>(availableEnumValues));
    }
}
