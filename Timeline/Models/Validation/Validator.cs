using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel.DataAnnotations;
using Timeline.Helpers;

namespace Timeline.Models.Validation
{
    /// <summary>
    /// Generate a message from a localizer factory.
    /// If localizerFactory is null, it should return a neutral-cultural message.
    /// </summary>
    /// <param name="localizerFactory">The localizer factory. Could be null.</param>
    /// <returns>The message.</returns>
    public delegate string ValidationMessageGenerator(IStringLocalizerFactory? localizerFactory);

    /// <summary>
    /// A validator to validate value.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Validate given value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>Validation success or not and the message generator.</returns>
        (bool, ValidationMessageGenerator) Validate(object? value);
    }

    /// <summary>
    /// Convenient base class for validator.
    /// </summary>
    /// <typeparam name="T">The type of accepted value.</typeparam>
    /// <remarks>
    /// Subclass should override <see cref="DoValidate(T, out string)"/> to do the real validation.
    /// This class will check the nullity and type of value. If value is null or not of type <typeparamref name="T"/>
    /// it will return false and not call <see cref="DoValidate(T, out string)"/>.
    /// 
    /// If you want some other behaviours, write the validator from scratch.
    /// </remarks>
    public abstract class Validator<T> : IValidator
    {
        public (bool, ValidationMessageGenerator) Validate(object? value)
        {
            if (value == null)
            {
                return (false, factory =>
                    factory?.Create("Models.Validation.Validator")?["ValidatorMessageNull"]
                    ?? Resources.Models.Validation.Validator.InvariantValidatorMessageNull
                );
            }

            if (value is T v)
            {
                return DoValidate(v);
            }
            else
            {
                return (false, factory =>
                    factory?.Create("Models.Validation.Validator")?["ValidatorMessageBadType", typeof(T).FullName]
                    ?? Resources.Models.Validation.Validator.InvariantValidatorMessageBadType);
            }
        }

        protected static ValidationMessageGenerator SuccessMessageGenerator { get; } = factory =>
            factory?.Create("Models.Validation.Validator")?["ValidatorMessageSuccess"] ?? Resources.Models.Validation.Validator.InvariantValidatorMessageSuccess;

        protected abstract (bool, ValidationMessageGenerator) DoValidate(T value);
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class ValidateWithAttribute : ValidationAttribute
    {
        private readonly IValidator _validator;

        /// <summary>
        /// Create with a given validator.
        /// </summary>
        /// <param name="validator">The validator used to validate.</param>
        public ValidateWithAttribute(IValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Create the validator with default constructor.
        /// </summary>
        /// <param name="validatorType">The type of the validator.</param>
        public ValidateWithAttribute(Type validatorType)
        {
            if (validatorType == null)
                throw new ArgumentNullException(nameof(validatorType));

            if (!typeof(IValidator).IsAssignableFrom(validatorType))
                throw new ArgumentException(
                Resources.Models.Validation.Validator.ValidateWithAttributeNotValidator,
                nameof(validatorType));

            try
            {
                _validator = (Activator.CreateInstance(validatorType) as IValidator)!;
            }
            catch (Exception e)
            {
                throw new ArgumentException(
                    Resources.Models.Validation.Validator.ValidateWithAttributeCreateFail, e);
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var (result, messageGenerator) = _validator.Validate(value);
            if (result)
            {
                return ValidationResult.Success;
            }
            else
            {
                var localizerFactory = validationContext.GetRequiredService<IStringLocalizerFactory>();
                return new ValidationResult(messageGenerator(localizerFactory));
            }
        }
    }
}
