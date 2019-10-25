using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel.DataAnnotations;
using Timeline.Helpers;
using static Timeline.Resources.Models.Validation.Validator;

namespace Timeline.Models.Validation
{
    /// <summary>
    /// A validator to validate value.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Validate given value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>Validation success or not and message.</returns>
        (bool, string) Validate(object? value);
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
        public (bool, string) Validate(object? value)
        {
            if (value == null)
            {
                return (false, ValidatorMessageNull);
            }

            if (value is T v)
            {
                return DoValidate(v);
            }
            else
            {
                return (false, ValidatorMessageBadType);
            }
        }

        protected static string GetSuccessMessage() => ValidatorMessageSuccess;

        protected abstract (bool, string) DoValidate(T value);
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
                throw new ArgumentException(ValidateWithAttributeExceptionNotValidator, nameof(validatorType));

            try
            {
                _validator = (Activator.CreateInstance(validatorType) as IValidator)!;
            }
            catch (Exception e)
            {
                throw new ArgumentException(ValidateWithAttributeExceptionCreateFail, e);
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var (result, message) = _validator.Validate(value);
            if (result)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(message);
            }
        }
    }
}
